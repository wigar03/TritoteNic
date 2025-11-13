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
    public class ProductoDto
    {
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public decimal PrecioProducto { get; set; }
        public int StockProducto { get; set; }
        public string? EstadoProducto { get; set; }
        public string? ImagenProducto { get; set; }
        public int IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
    }
}
