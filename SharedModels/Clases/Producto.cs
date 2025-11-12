using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Clases
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Required, StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; }

        [StringLength(255)]
        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Required]
        [Column("precio", TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required]
        [Column("stock")]
        public int Stock { get; set; }

        [Required, StringLength(50)]
        [Column("categoria")]
        public string Categoria { get; set; }

        [Required, StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } // Activo, Inactivo

        public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
    }
}
