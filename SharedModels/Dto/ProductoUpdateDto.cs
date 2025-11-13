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
    public class ProductoUpdateDto
    {
        [Required]
        public int IdProducto { get; set; }

        [Required, StringLength(100)]
        public string? NombreProducto { get; set; }

        [StringLength(255)]
        public string? DescripcionProducto { get; set; }

        [Required]
        public decimal PrecioProducto { get; set; }

        [Required]
        public int StockProducto { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        [Required, StringLength(20)]
        public string? EstadoProducto { get; set; }

        [StringLength(255)]
        public string? ImagenProducto { get; set; }
    }
}
