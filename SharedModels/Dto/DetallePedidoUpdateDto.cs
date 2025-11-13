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
    public class DetallePedidoUpdateDto
    {
        [Required]
        public int IdDetalle { get; set; }

        [Required]
        public int IdPedido { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int CantidadProducto { get; set; }

        [Required]
        public decimal PrecioUnitarioProducto { get; set; }

        [Required]
        public decimal SubtotalProducto { get; set; }
    }


}
