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
        public string Nombre_Usuario { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(255)]
        public string Contraseña { get; set; }

        [Required, StringLength(50)]
        public string Rol { get; set; } // Administrador, Vendedor, Analista

        [Required, StringLength(20)]
        public string Estado { get; set; } // Activo, Inactivo

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con pedidos (1 Usuario puede registrar muchos pedidos)
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
