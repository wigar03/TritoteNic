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
    public class UsuarioCreateDto
    {
        [Required, StringLength(100)]
        public string? NombreUsuario { get; set; }

        [Required, StringLength(100)]
        public string? EmailUsuario { get; set; }

        [Required, StringLength(255)]
        public string? ContrasenaUsuario { get; set; }

        [Required]
        public int IdRol { get; set; }

        [Required, StringLength(20)]
        public string? EstadoUsuario { get; set; }
    }
}
