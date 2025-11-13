using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Dto
{
    public class MetodoPagoUpdateDto
    {
        [Required]
        public int IdMetodoPago { get; set; }

        [Required, StringLength(50)]
        public string? NombreMetodoPago { get; set; }

        [StringLength(255)]
        public string? DescripcionMetodoPago { get; set; }
    }
}
