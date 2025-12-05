using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Dto;
using TritoteNic.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(TritoteContext.TritoteConext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            try
            {
                _logger.LogInformation("Obteniendo datos del dashboard");

                var hoy = DateTime.UtcNow.Date;
                var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var inicioMesAnterior = inicioMes.AddMonths(-1);
                var finMesAnterior = inicioMes.AddDays(-1);

                // Ventas del día
                var ventasDia = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date == hoy)
                    .SumAsync(p => (decimal?)p.TotalPedido) ?? 0;

                // Ventas del día anterior
                var ventasDiaAnterior = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date == hoy.AddDays(-1))
                    .SumAsync(p => (decimal?)p.TotalPedido) ?? 0;

                // Ventas de la semana (lunes a domingo)
                var ventasSemana = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date >= inicioSemana && p.FechaPedido.Date <= hoy)
                    .SumAsync(p => (decimal?)p.TotalPedido) ?? 0;

                // Ventas de la semana anterior
                var inicioSemanaAnterior = inicioSemana.AddDays(-7);
                var finSemanaAnterior = inicioSemana.AddDays(-1);
                var ventasSemanaAnterior = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date >= inicioSemanaAnterior && p.FechaPedido.Date <= finSemanaAnterior)
                    .SumAsync(p => (decimal?)p.TotalPedido) ?? 0;

                // Ventas del mes
                var ventasMes = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date >= inicioMes && p.FechaPedido.Date <= hoy)
                    .SumAsync(p => (decimal?)p.TotalPedido) ?? 0;

                // Ventas del mes anterior
                var ventasMesAnterior = await _context.Pedidos
                    .Where(p => p.FechaPedido.Date >= inicioMesAnterior && p.FechaPedido.Date <= finMesAnterior)
                    .SumAsync(p => (decimal?)p.TotalPedido) ?? 0;

                // Calcular porcentajes de cambio
                var porcentajeCambioDia = CalcularPorcentajeCambio(ventasDiaAnterior, ventasDia);
                var porcentajeCambioSemana = CalcularPorcentajeCambio(ventasSemanaAnterior, ventasSemana);
                var porcentajeCambioMes = CalcularPorcentajeCambio(ventasMesAnterior, ventasMes);

                var ventasKpi = new VentasKpiDto
                {
                    VentasDia = ventasDia,
                    VentasSemana = ventasSemana,
                    VentasMes = ventasMes,
                    PorcentajeCambioDia = porcentajeCambioDia,
                    PorcentajeCambioSemana = porcentajeCambioSemana,
                    PorcentajeCambioMes = porcentajeCambioMes
                };

                // Obtener estados de pedido (asumiendo que existen estados como "Activo", "Pendiente", "En Proceso")
                List<SharedModels.Clases.EstadoPedido> estados = new List<SharedModels.Clases.EstadoPedido>();
                try
                {
                    estados = await _context.EstadosPedidos.ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al obtener estados de pedido (continuando sin ellos): {ex.Message}");
                    estados = new List<SharedModels.Clases.EstadoPedido>();
                }
                
                // Buscar estados por nombre común (ajustar según los nombres reales en tu BD)
                var estadoActivo = estados.FirstOrDefault(e => 
                    !string.IsNullOrEmpty(e.NombreEstadoPedido) &&
                    (e.NombreEstadoPedido.Contains("Activo", StringComparison.OrdinalIgnoreCase) ||
                     e.NombreEstadoPedido.Contains("Completado", StringComparison.OrdinalIgnoreCase) ||
                     e.NombreEstadoPedido.Contains("Entregado", StringComparison.OrdinalIgnoreCase)));

                var estadoPendiente = estados.FirstOrDefault(e => 
                    !string.IsNullOrEmpty(e.NombreEstadoPedido) &&
                    (e.NombreEstadoPedido.Contains("Pendiente", StringComparison.OrdinalIgnoreCase) ||
                     e.NombreEstadoPedido.Contains("Nuevo", StringComparison.OrdinalIgnoreCase)));

                var estadoEnProceso = estados.FirstOrDefault(e => 
                    !string.IsNullOrEmpty(e.NombreEstadoPedido) &&
                    (e.NombreEstadoPedido.Contains("Proceso", StringComparison.OrdinalIgnoreCase) ||
                     e.NombreEstadoPedido.Contains("Preparando", StringComparison.OrdinalIgnoreCase)));

                var pedidosActivos = estadoActivo != null
                    ? await _context.Pedidos.CountAsync(p => p.IdEstadoPedido == estadoActivo.IdEstadoPedido)
                    : 0;

                var pedidosPendientes = estadoPendiente != null
                    ? await _context.Pedidos.CountAsync(p => p.IdEstadoPedido == estadoPendiente.IdEstadoPedido)
                    : 0;

                var pedidosEnProceso = estadoEnProceso != null
                    ? await _context.Pedidos.CountAsync(p => p.IdEstadoPedido == estadoEnProceso.IdEstadoPedido)
                    : 0;

                var totalPedidos = await _context.Pedidos.CountAsync();

                var pedidosKpi = new PedidosKpiDto
                {
                    PedidosActivos = pedidosActivos,
                    PedidosPendientes = pedidosPendientes,
                    PedidosEnProceso = pedidosEnProceso,
                    TotalPedidos = totalPedidos
                };

                // Alertas: productos con stock bajo
                List<AlertaDto> productosStockBajo = new List<AlertaDto>();
                try
                {
                    productosStockBajo = await _context.Productos
                        .Where(p => p.StockProducto < 10 && p.EstadoProducto == "Activo")
                        .Select(p => new AlertaDto
                        {
                            Tipo = "StockBajo",
                            Mensaje = $"Producto {p.NombreProducto ?? "Sin nombre"} tiene stock bajo: {p.StockProducto} unidades",
                            IdProducto = p.IdProducto,
                            NombreProducto = p.NombreProducto,
                            StockActual = p.StockProducto
                        })
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al obtener productos con stock bajo (continuando sin ellos): {ex.Message}");
                    productosStockBajo = new List<AlertaDto>();
                }

                // Alertas: pedidos retrasados (más de 3 días sin completar y no en estado completado)
                List<AlertaDto> pedidosRetrasados = new List<AlertaDto>();
                try
                {
                    var fechaLimite = hoy.AddDays(-3);
                    var estadosNoCompletados = estados
                        .Where(e => !string.IsNullOrEmpty(e.NombreEstadoPedido) &&
                                    !e.NombreEstadoPedido.Contains("Completado", StringComparison.OrdinalIgnoreCase) &&
                                    !e.NombreEstadoPedido.Contains("Entregado", StringComparison.OrdinalIgnoreCase) &&
                                    !e.NombreEstadoPedido.Contains("Cancelado", StringComparison.OrdinalIgnoreCase))
                        .Select(e => e.IdEstadoPedido)
                        .ToList();

                    if (estadosNoCompletados.Any())
                    {
                        pedidosRetrasados = await _context.Pedidos
                            .Where(p => estadosNoCompletados.Contains(p.IdEstadoPedido) && 
                                        p.FechaPedido.Date < fechaLimite)
                            .Select(p => new AlertaDto
                            {
                                Tipo = "PedidoRetrasado",
                                Mensaje = $"Pedido #{p.IdPedido} tiene {((hoy - p.FechaPedido.Date).Days)} días de retraso",
                                IdPedido = p.IdPedido,
                                DiasRetraso = (hoy - p.FechaPedido.Date).Days
                            })
                            .ToListAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al obtener pedidos retrasados (continuando sin ellos): {ex.Message}");
                    pedidosRetrasados = new List<AlertaDto>();
                }

                var alertas = new List<AlertaDto>();
                alertas.AddRange(productosStockBajo);
                alertas.AddRange(pedidosRetrasados);

                // Ventas de los últimos 7 días
                List<VentasDiariasDto> ventasDiarias;
                try
                {
                    var fechaInicio = hoy.AddDays(-6);
                    ventasDiarias = await _context.Pedidos
                        .Where(p => p.FechaPedido.Date >= fechaInicio && 
                                   p.FechaPedido.Date <= hoy)
                        .GroupBy(p => p.FechaPedido.Date)
                        .Select(g => new VentasDiariasDto
                        {
                            Fecha = g.Key.Date.ToUniversalTime(),
                            TotalVentas = g.Sum(p => p.TotalPedido),
                            CantidadPedidos = g.Count()
                        })
                        .OrderBy(v => v.Fecha)
                        .ToListAsync();

                    // Completar días sin ventas con 0
                    for (var fecha = fechaInicio; fecha <= hoy; fecha = fecha.AddDays(1))
                    {
                        var fechaUtc = new DateTime(fecha.Year, fecha.Month, fecha.Day, 0, 0, 0, DateTimeKind.Utc);
                        if (!ventasDiarias.Any(v => v.Fecha.Date == fechaUtc.Date))
                        {
                            ventasDiarias.Add(new VentasDiariasDto
                            {
                                Fecha = fechaUtc,
                                TotalVentas = 0,
                                CantidadPedidos = 0
                            });
                        }
                    }
                    ventasDiarias = ventasDiarias.OrderBy(v => v.Fecha).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al obtener ventas diarias (continuando sin ellas): {ex.Message}");
                    ventasDiarias = new List<VentasDiariasDto>();
                    var fechaInicio = hoy.AddDays(-6);
                    // Completar con días vacíos
                    for (var fecha = fechaInicio; fecha <= hoy; fecha = fecha.AddDays(1))
                    {
                        var fechaUtc = new DateTime(fecha.Year, fecha.Month, fecha.Day, 0, 0, 0, DateTimeKind.Utc);
                        ventasDiarias.Add(new VentasDiariasDto
                        {
                            Fecha = fechaUtc,
                            TotalVentas = 0,
                            CantidadPedidos = 0
                        });
                    }
                }

                // Top 5 productos más vendidos del mes
                var productosMasVendidos = new List<ProductoVendidoDto>();
                try
                {
                    var detallesConProductos = await _context.DetallesPedido
                        .Include(d => d.Pedido)
                        .Include(d => d.Producto)
                        .Where(d => d.Pedido != null && 
                                   d.Pedido.FechaPedido.Date >= inicioMes && 
                                   d.Pedido.FechaPedido.Date <= hoy &&
                                   d.Producto != null)
                        .ToListAsync();

                    productosMasVendidos = detallesConProductos
                        .GroupBy(d => new { d.IdProducto, NombreProducto = d.Producto?.NombreProducto ?? "Sin nombre" })
                        .Select(g => new ProductoVendidoDto
                        {
                            IdProducto = g.Key.IdProducto,
                            NombreProducto = g.Key.NombreProducto,
                            CantidadVendida = g.Sum(d => d.CantidadProducto),
                            TotalVentas = g.Sum(d => d.SubtotalProducto)
                        })
                        .OrderByDescending(p => p.CantidadVendida)
                        .Take(5)
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al obtener productos más vendidos (continuando sin ellos): {ex.Message}");
                    productosMasVendidos = new List<ProductoVendidoDto>();
                }

                var dashboard = new DashboardDto
                {
                    VentasKpi = ventasKpi ?? new VentasKpiDto(),
                    PedidosKpi = pedidosKpi ?? new PedidosKpiDto(),
                    Alertas = alertas ?? new List<AlertaDto>(),
                    VentasDiarias = ventasDiarias ?? new List<VentasDiariasDto>(),
                    ProductosMasVendidos = productosMasVendidos ?? new List<ProductoVendidoDto>()
                };

                _logger.LogInformation("Dashboard creado exitosamente");
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener datos del dashboard: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "Error interno del servidor al obtener los datos del dashboard.", error = ex.Message });
            }
        }

        private decimal CalcularPorcentajeCambio(decimal valorAnterior, decimal valorActual)
        {
            if (valorAnterior == 0)
                return valorActual > 0 ? 100 : 0;

            return Math.Round(((valorActual - valorAnterior) / valorAnterior) * 100, 2);
        }
    }
}
