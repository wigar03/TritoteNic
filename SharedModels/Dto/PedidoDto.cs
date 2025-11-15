using SharedModels.Clases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class PedidoDto
    {
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public int IdEstadoPedido { get; set; }
        public string? NombreEstadoPedido { get; set; }
        public int IdMetodoPago { get; set; }
        public string? NombreMetodoPago { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal? SubtotalPedido { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalPedido { get; set; }
        public List<DetallePedidoDto>? Detalles { get; set; }
    }
}
