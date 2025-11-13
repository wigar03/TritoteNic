using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Clases
{
    [Table("MetodosPago")]
    public class MetodoPago
    {
        [Key]
        [Column("id_metodo_pago")]
        public int IdMetodoPago { get; set; }

        [Required, StringLength(50)]
        public string NombreMetodoPago { get; set; }

        [StringLength(255)]
        public string DescripcionMetodoPago { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
