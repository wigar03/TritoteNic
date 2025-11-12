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
    public class DetallePedidoDto
    {
        [Required]
        [ForeignKey("Pedido")]
        public int IdPedido { get; set; }

        public Pedido Pedido { get; set; }

        [Required]
        [ForeignKey("Producto")]
        public int IdProducto { get; set; }

        public Producto Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}
