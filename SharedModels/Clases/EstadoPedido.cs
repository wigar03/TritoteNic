using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Clases
{
    [Table("EstadosPedido")]
    public class EstadoPedido
    {
        [Key]
        [Column("id_estado_pedido")]
        public int IdEstadoPedido { get; set; }

        [Required, StringLength(50)]
        public string NombreEstadoPedido { get; set; }

        [StringLength(255)]
        public string DescripcionEstadoPedido { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
