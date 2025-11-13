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
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? EmailUsuario { get; set; }
        public string? EstadoUsuario { get; set; }
        public DateTime FechaCreacionUsuario { get; set; }
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
    }
}
