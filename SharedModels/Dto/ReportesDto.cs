using System;
using System.Collections.Generic;

namespace SharedModels.Dto
{
    public class AnalisisCompletoDto
    {
        public ComparativaVentasDto ComparativaVentas { get; set; }
        public List<TendenciaColorDto> TendenciasColor { get; set; }
        public List<TendenciaTemporadaDto> TendenciasTemporada { get; set; }
        public List<RotacionProductoDto> ProductosRotacion { get; set; }
    }

    public class ComparativaVentasDto
    {
        public List<ReporteVentasDto> VentasSemanales { get; set; }
        public decimal TotalPeriodoActual { get; set; }
        public decimal TotalPeriodoAnterior { get; set; }
        public decimal PorcentajeCambio { get; set; }
    }

    public class ReporteVentasDto
    {
        public string Periodo { get; set; } // "Semana 1", "Semana 2", etc.
        public decimal VentasActuales { get; set; }
        public decimal VentasAnteriores { get; set; }
        public decimal PorcentajeCambio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class TendenciaColorDto
    {
        public string Color { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal PorcentajeVentas { get; set; }
        public int CantidadProductos { get; set; }
    }

    public class TendenciaTemporadaDto
    {
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    public class RotacionProductoDto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal Rotacion { get; set; } // Rotación = Cantidad vendida / Stock inicial (o período)
        public string TipoRotacion { get; set; } // "Alta" o "Baja"
    }

    public class TendenciasColorDto
    {
        public List<TendenciaColorDto> Tendencias { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }
}
