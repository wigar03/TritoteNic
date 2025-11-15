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
    public class ClienteDto
    {
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public string? DireccionCliente { get; set; }
        public string? EmailCliente { get; set; }
        public string? CategoriaCliente { get; set; }
        public decimal TotalGastado { get; set; }
        public DateTime? FechaUltimoPedido { get; set; }
        public int? TotalPedidos { get; set; }
    }
}
