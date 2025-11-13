using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class CategoriaCreateDto
    {
        [Required, StringLength(50)]
        public string? NombreCategoria { get; set; }

        [StringLength(255)]
        public string? DescripcionCategoria { get; set; }
    }
}
