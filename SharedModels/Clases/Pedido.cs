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
        [ForeignKey("EstadoPedido")]
        [Column("id_estado_pedido")]
        public int IdEstadoPedido { get; set; }

        public EstadoPedido EstadoPedido { get; set; }

        [Required]
        [ForeignKey("MetodoPago")]
        [Column("id_metodo_pago")]
        public int IdMetodoPago { get; set; }

        public MetodoPago MetodoPago { get; set; }

        [Required]
        [Column("fecha_pedido")]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required]
        [Column("total", TypeName = "decimal(10,2)")]
        public decimal TotalPedido { get; set; }

        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
