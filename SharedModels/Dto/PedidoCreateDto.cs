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
    public class PedidoCreateDto
    {
        [Required]
        [ForeignKey("Cliente")]
        public int IdCliente { get; set; }

        public Cliente Cliente { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        public Usuario Usuario { get; set; }

        [Required]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required, StringLength(30)]
        public string Estado { get; set; } // Pendiente, En Proceso, Completado, Retrasado

        [Required, StringLength(50)]
        public string MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia

        [Required]
        public decimal Total { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
