using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class EstadoPedidoCreateDto
    {
        [Required, StringLength(50)]
        public string? NombreEstadoPedido { get; set; }

        [StringLength(255)]
        public string? DescripcionEstadoPedido { get; set; }
    }
}
