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
    public class PedidoUpdateDto
    {
        [Required]
        public int IdPedido { get; set; }

        [Required]
        public int IdEstadoPedido { get; set; }

        [Required]
        public int IdMetodoPago { get; set; }

        [Required]
        public decimal TotalPedido { get; set; }
    }
}
