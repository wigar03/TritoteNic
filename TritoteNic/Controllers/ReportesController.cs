using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Dto;
using TritoteNic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<ReportesController> _logger;

        // Lista de colores comunes para extraer de los nombres de productos
        private static readonly string[] Colores = new[]
        {
            "rojo", "azul", "verde", "amarillo", "negro", "blanco", "gris", "rosa",
            "morado", "violeta", "naranja", "marron", "beige", "dorado", "plateado",
            "cyan", "magenta", "turquesa", "coral", "crema", "bronce", "cobre"
        };

        public ReportesController(TritoteContext.TritoteConext context, ILogger<ReportesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("analisis-completo")]
        public async Task<ActionResult<AnalisisCompletoDto>> GetAnalisisCompleto(
            [FromQuery] int? mes = null,
            [FromQuery] int? año = null)
        {
            try
            {
                var hoy = DateTime.Now;
                var mesAnalisis = mes ?? hoy.Month;
                var añoAnalisis = año ?? hoy.Year;

                _logger.LogInformation($"Obteniendo análisis completo para {mesAnalisis}/{añoAnalisis}");

                var inicioMes = new DateTime(añoAnalisis, mesAnalisis, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);
                var inicioMesAnterior = inicioMes.AddMonths(-1);
                var finMesAnterior = inicioMes.AddDays(-1);

                // Comparativa de ventas por semanas
                var ventasSemanales = new List<ReporteVentasDto>();
                var fechaActual = inicioMes;
                int semanaNumero = 1;

                while (fechaActual <= finMes)
                {
                    var finSemana = fechaActual.AddDays(6) > finMes ? finMes : fechaActual.AddDays(6);
                    var inicioSemanaAnterior = fechaActual.AddDays(-7);
                    var finSemanaAnterior = fechaActual.AddDays(-1);

                    var ventasActuales = await _context.Pedidos
                        .Where(p => p.FechaPedido.Date >= fechaActual.Date && p.FechaPedido.Date <= finSemana.Date)
                        .SumAsync(p => p.TotalPedido);

                    var ventasAnteriores = await _context.Pedidos
                        .Where(p => p.FechaPedido.Date >= inicioSemanaAnterior.Date && 
                                   p.FechaPedido.Date <= finSemanaAnterior.Date)
                        .SumAsync(p => p.TotalPedido);

                    var porcentajeCambio = CalcularPorcentajeCambio(ventasAnteriores, ventasActuales);

                    ventasSemanales.Add(new ReporteVentasDto
                    {
                        Periodo = $"Semana {semanaNumero}",
                        VentasActuales = ventasActuales,
                        VentasAnteriores = ventasAnteriores,
                        PorcentajeCambio = porcentajeCambio,
                        FechaInicio = fechaActual,
                        FechaFin = finSemana
                    });

                    fechaActual = finSemana.AddDays(1);
                    semanaNumero++;
                }

                var totalPeriodoActual = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date >= inicioMes.Date && p.FechaPedido.Date <= finMes.Date)
                    .SumAsync(p => p.TotalPedido);

                var totalPeriodoAnterior = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date >= inicioMesAnterior.Date && 
                               p.FechaPedido.Date <= finMesAnterior.Date)
                    .SumAsync(p => p.TotalPedido);

                var porcentajeCambioTotal = CalcularPorcentajeCambio(totalPeriodoAnterior, totalPeriodoActual);

                var comparativaVentas = new ComparativaVentasDto
                {
                    VentasSemanales = ventasSemanales,
                    TotalPeriodoActual = totalPeriodoActual,
                    TotalPeriodoAnterior = totalPeriodoAnterior,
                    PorcentajeCambio = porcentajeCambioTotal
                };

                // Tendencias de color (extraer de nombres de productos)
                var productosVendidos = await _context.DetallesPedido
                    .Where(d => d.Pedido.FechaPedido.Date >= inicioMes.Date && 
                               d.Pedido.FechaPedido.Date <= finMes.Date)
                    .Include(d => d.Producto)
                    .Select(d => new
                    {
                        NombreProducto = d.Producto.NombreProducto,
                        Cantidad = d.CantidadProducto,
                        Subtotal = d.SubtotalProducto
                    })
                    .ToListAsync();

                var tendenciasColor = productosVendidos
                    .SelectMany(p => ExtraerColores(p.NombreProducto)
                        .Select(color => new
                        {
                            Color = color,
                            Cantidad = p.Cantidad,
                            Subtotal = p.Subtotal
                        }))
                    .GroupBy(t => t.Color.ToLower())
                    .Select(g => new TendenciaColorDto
                    {
                        Color = Capitalizar(g.Key),
                        CantidadVendida = g.Sum(t => t.Cantidad),
                        TotalVentas = g.Sum(t => t.Subtotal),
                        CantidadProductos = g.Count()
                    })
                    .OrderByDescending(t => t.CantidadVendida)
                    .ToList();

                var totalVentasColor = tendenciasColor.Sum(t => t.TotalVentas);
                foreach (var tendencia in tendenciasColor)
                {
                    tendencia.PorcentajeVentas = totalVentasColor > 0
                        ? Math.Round((tendencia.TotalVentas / totalVentasColor) * 100, 2)
                        : 0;
                }

                // Tendencias por temporada (meses)
                var tendenciasTemporada = await _context.Pedidos
                    .Where(p => p.FechaPedido.Year == añoAnalisis)
                    .GroupBy(p => p.FechaPedido.Month)
                    .Select(g => new TendenciaTemporadaDto
                    {
                        Mes = g.Key,
                        NombreMes = new DateTime(añoAnalisis, g.Key, 1).ToString("MMMM"),
                        TotalVentas = g.Sum(p => p.TotalPedido),
                        CantidadPedidos = g.Count(),
                        PromedioVenta = g.Average(p => p.TotalPedido)
                    })
                    .OrderBy(t => t.Mes)
                    .ToListAsync();

                // Rotación de productos (mayor y menor)
                var productosConStock = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.EstadoProducto == "Activo")
                    .ToListAsync();

                var productosRotacion = new List<RotacionProductoDto>();

                foreach (var producto in productosConStock)
                {
                    var ventasProducto = await _context.DetallesPedido
                        .Where(d => d.IdProducto == producto.IdProducto &&
                                   d.Pedido.FechaPedido.Date >= inicioMes.Date &&
                                   d.Pedido.FechaPedido.Date <= finMes.Date)
                        .SumAsync(d => d.CantidadProducto);

                    var totalVentasProducto = await _context.DetallesPedido
                        .Where(d => d.IdProducto == producto.IdProducto &&
                                   d.Pedido.FechaPedido.Date >= inicioMes.Date &&
                                   d.Pedido.FechaPedido.Date <= finMes.Date)
                        .SumAsync(d => d.SubtotalProducto);

                    // Rotación = cantidad vendida / stock actual (ajustar según necesidad)
                    var rotacion = producto.StockProducto > 0
                        ? Math.Round((decimal)ventasProducto / producto.StockProducto, 2)
                        : ventasProducto > 0 ? 100 : 0; // Si no hay stock pero hay ventas, rotación alta

                    productosRotacion.Add(new RotacionProductoDto
                    {
                        IdProducto = producto.IdProducto,
                        NombreProducto = producto.NombreProducto,
                        Categoria = producto.Categoria?.NombreCategoria ?? "Sin categoría",
                        CantidadVendida = (int)ventasProducto,
                        TotalVentas = totalVentasProducto,
                        Rotacion = rotacion,
                        TipoRotacion = rotacion >= 1 ? "Alta" : "Baja"
                    });
                }

                // Ordenar por rotación
                var productosAltaRotacion = productosRotacion
                    .OrderByDescending(p => p.Rotacion)
                    .Take(10)
                    .ToList();

                var productosBajaRotacion = productosRotacion
                    .Where(p => p.CantidadVendida == 0 || p.Rotacion < 0.1m)
                    .OrderBy(p => p.Rotacion)
                    .Take(10)
                    .ToList();

                var todosProductosRotacion = productosAltaRotacion
                    .Concat(productosBajaRotacion.Where(p => !productosAltaRotacion.Any(a => a.IdProducto == p.IdProducto)))
                    .ToList();

                var analisis = new AnalisisCompletoDto
                {
                    ComparativaVentas = comparativaVentas,
                    TendenciasColor = tendenciasColor,
                    TendenciasTemporada = tendenciasTemporada,
                    ProductosRotacion = todosProductosRotacion
                };

                return Ok(analisis);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener análisis completo: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al obtener el análisis completo.");
            }
        }

        [HttpGet("tendencias-color")]
        public async Task<ActionResult<TendenciasColorDto>> GetTendenciasColor(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var hoy = DateTime.Now;
                var desde = fechaDesde ?? hoy.AddMonths(-6);
                var hasta = fechaHasta ?? hoy;

                _logger.LogInformation($"Obteniendo tendencias de color desde {desde:yyyy-MM-dd} hasta {hasta:yyyy-MM-dd}");

                var productosVendidos = await _context.DetallesPedido
                    .Where(d => d.Pedido.FechaPedido.Date >= desde.Date && 
                               d.Pedido.FechaPedido.Date <= hasta.Date)
                    .Include(d => d.Producto)
                    .Select(d => new
                    {
                        NombreProducto = d.Producto.NombreProducto,
                        Cantidad = d.CantidadProducto,
                        Subtotal = d.SubtotalProducto
                    })
                    .ToListAsync();

                var tendencias = productosVendidos
                    .SelectMany(p => ExtraerColores(p.NombreProducto)
                        .Select(color => new
                        {
                            Color = color,
                            Cantidad = p.Cantidad,
                            Subtotal = p.Subtotal
                        }))
                    .GroupBy(t => t.Color.ToLower())
                    .Select(g => new TendenciaColorDto
                    {
                        Color = Capitalizar(g.Key),
                        CantidadVendida = g.Sum(t => t.Cantidad),
                        TotalVentas = g.Sum(t => t.Subtotal),
                        CantidadProductos = g.Count()
                    })
                    .OrderByDescending(t => t.CantidadVendida)
                    .ToList();

                var totalVentas = tendencias.Sum(t => t.TotalVentas);
                foreach (var tendencia in tendencias)
                {
                    tendencia.PorcentajeVentas = totalVentas > 0
                        ? Math.Round((tendencia.TotalVentas / totalVentas) * 100, 2)
                        : 0;
                }

                var resultado = new TendenciasColorDto
                {
                    Tendencias = tendencias,
                    FechaDesde = desde,
                    FechaHasta = hasta
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tendencias de color: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al obtener las tendencias de color.");
            }
        }

        private List<string> ExtraerColores(string nombreProducto)
        {
            if (string.IsNullOrWhiteSpace(nombreProducto))
                return new List<string>();

            var nombreLower = nombreProducto.ToLower();
            var coloresEncontrados = new List<string>();

            foreach (var color in Colores)
            {
                if (nombreLower.Contains(color, StringComparison.OrdinalIgnoreCase))
                {
                    coloresEncontrados.Add(color);
                }
            }

            // Si no se encontró ningún color, retornar lista vacía o un color genérico
            return coloresEncontrados.Distinct().ToList();
        }

        private string Capitalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            return char.ToUpper(texto[0]) + texto.Substring(1).ToLower();
        }

        private decimal CalcularPorcentajeCambio(decimal valorAnterior, decimal valorActual)
        {
            if (valorAnterior == 0)
                return valorActual > 0 ? 100 : 0;

            return Math.Round(((valorActual - valorAnterior) / valorAnterior) * 100, 2);
        }
    }
}
