using System;
using System.Collections.Generic;

namespace SharedModels.Dto
{
    public class DashboardDto
    {
        public VentasKpiDto VentasKpi { get; set; } = new VentasKpiDto();
        public PedidosKpiDto PedidosKpi { get; set; } = new PedidosKpiDto();
        public List<AlertaDto> Alertas { get; set; } = new List<AlertaDto>();
        public List<VentasDiariasDto> VentasDiarias { get; set; } = new List<VentasDiariasDto>();
        public List<ProductoVendidoDto> ProductosMasVendidos { get; set; } = new List<ProductoVendidoDto>();
    }

    public class VentasKpiDto
    {
        public decimal VentasDia { get; set; }
        public decimal VentasSemana { get; set; }
        public decimal VentasMes { get; set; }
        public decimal PorcentajeCambioDia { get; set; }
        public decimal PorcentajeCambioSemana { get; set; }
        public decimal PorcentajeCambioMes { get; set; }
    }

    public class PedidosKpiDto
    {
        public int PedidosActivos { get; set; }
        public int PedidosPendientes { get; set; }
        public int PedidosEnProceso { get; set; }
        public int TotalPedidos { get; set; }
    }

    public class AlertaDto
    {
        public string? Tipo { get; set; } // "StockBajo" o "PedidoRetrasado"
        public string? Mensaje { get; set; }
        public int? IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int? StockActual { get; set; }
        public int? IdPedido { get; set; }
        public int? DiasRetraso { get; set; }
    }

    public class VentasDiariasDto
    {
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadPedidos { get; set; }
    }

    public class ProductoVendidoDto
    {
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }
}
