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
    public class PedidoCreateDto
    {
        [Required]
        public int IdCliente { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdEstadoPedido { get; set; }

        [Required]
        public int IdMetodoPago { get; set; }

        [Required]
        public decimal TotalPedido { get; set; }
    }
}
