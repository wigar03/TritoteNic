using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Clases
{
    [Table("DetallesPedido")]
    public class DetallePedido
    {
        [Key]
        [Column("id_detalle")]
        public int IdDetalle { get; set; }

        [Required]
        [ForeignKey("Pedido")]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        public Pedido Pedido { get; set; }

        [Required]
        [ForeignKey("Producto")]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        public Producto Producto { get; set; }

        [Required]
        public int CantidadProducto { get; set; }

        [Required]
        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitarioProducto { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal SubtotalProducto { get; set; }
    }
}
