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
    public class ClienteUpdateDto
    {
        [Required]
        public int IdCliente { get; set; }

        [Required, StringLength(100)]
        public string? NombreCliente { get; set; }

        [StringLength(15)]
        public string? TelefonoCliente { get; set; }

        [StringLength(255)]
        public string? DireccionCliente { get; set; }

        [StringLength(100)]
        public string? EmailCliente { get; set; }
    }
}
