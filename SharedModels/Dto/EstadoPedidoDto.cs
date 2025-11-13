using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class EstadoPedidoDto
    {
        public int IdEstadoPedido { get; set; }
        public string? NombreEstadoPedido { get; set; }
        public string? DescripcionEstadoPedido { get; set; }
    }
}
