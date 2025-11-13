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
        public string NombreProducto { get; set; }

        [StringLength(255)]
        public string DescripcionProducto { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioProducto { get; set; }

        [Required]
        public int StockProducto { get; set; }

        [Required]
        [ForeignKey("Categoria")]
        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        public Categoria Categoria { get; set; }

        [Required, StringLength(20)]
        public string EstadoProducto { get; set; } // Activo / Inactivo

        [StringLength(255)]
        public string ImagenProducto { get; set; }

        public ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();
    }
}
