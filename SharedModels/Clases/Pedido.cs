using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Clases
{
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Required]
        [ForeignKey("Cliente")]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        public Cliente Cliente { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        public Usuario Usuario { get; set; }

        [Required]
        [Column("fecha_pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required, StringLength(30)]
        [Column("estado")]
        public string Estado { get; set; } // Pendiente, En Proceso, Completado, Retrasado

        [Required, StringLength(50)]
        [Column("metodo_pago")]
        public string MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia

        [Required]
        [Column("total", TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
