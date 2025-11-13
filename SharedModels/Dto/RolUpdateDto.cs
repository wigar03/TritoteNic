using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class RolUpdateDto
    {
        [Required]
        public int IdRol { get; set; }

        [Required, StringLength(50)]
        public string? NombreRol { get; set; }

        [StringLength(255)]
        public string? DescripcionRol { get; set; }
    }
}
