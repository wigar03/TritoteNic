using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class MetodoPagoDto
    {
        public int IdMetodoPago { get; set; }
        public string? NombreMetodoPago { get; set; }
        public string? DescripcionMetodoPago { get; set; }
    }
}
