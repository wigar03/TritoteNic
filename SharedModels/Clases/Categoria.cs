using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Clases
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Required, StringLength(50)]
        public string NombreCategoria { get; set; }

        [StringLength(255)]
        public string DescripcionCategoria { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
