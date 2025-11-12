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
    public class ClienteCreateDto
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(255)]
        public string Direccion { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        // Relación con pedidos
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
