using System;
using System.Collections.Generic;

namespace SharedModels.Dto
{
    public class AnalisisCompletoDto
    {
        public ComparativaVentasDto ComparativaVentas { get; set; } = new ComparativaVentasDto();
        public List<TendenciaColorDto> TendenciasColor { get; set; } = new List<TendenciaColorDto>();
        public List<TendenciaTemporadaDto> TendenciasTemporada { get; set; } = new List<TendenciaTemporadaDto>();
        public List<RotacionProductoDto> ProductosRotacion { get; set; } = new List<RotacionProductoDto>();
    }

    public class ComparativaVentasDto
    {
        public List<ReporteVentasDto> VentasSemanales { get; set; } = new List<ReporteVentasDto>();
        public decimal TotalPeriodoActual { get; set; }
        public decimal TotalPeriodoAnterior { get; set; }
        public decimal PorcentajeCambio { get; set; }
    }

    public class ReporteVentasDto
    {
        public string Periodo { get; set; } = string.Empty; // "Semana 1", "Semana 2", etc.
        public decimal VentasActuales { get; set; }
        public decimal VentasAnteriores { get; set; }
        public decimal PorcentajeCambio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class TendenciaColorDto
    {
        public string Color { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal PorcentajeVentas { get; set; }
        public int CantidadProductos { get; set; }
    }

    public class TendenciaTemporadaDto
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal TotalVentas { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    public class RotacionProductoDto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal Rotacion { get; set; } // Rotación = Cantidad vendida / Stock inicial (o período)
        public string TipoRotacion { get; set; } = string.Empty; // "Alta" o "Baja"
    }

    public class TendenciasColorDto
    {
        public List<TendenciaColorDto> Tendencias { get; set; } = new List<TendenciaColorDto>();
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }
}
