using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Clases
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Required, StringLength(100)]
        public string NombreCliente { get; set; }

        [StringLength(15)]
        public string TelefonoCliente { get; set; }

        [StringLength(255)]
        public string DireccionCliente { get; set; }

        [StringLength(100)]
        public string EmailCliente { get; set; }

        [StringLength(20)]
        public string? CategoriaCliente { get; set; } // VIP, Frecuente, Regular

        [Column("total_gastado", TypeName = "decimal(10,2)")]
        public decimal TotalGastado { get; set; } = 0;

        [Column("fecha_ultimo_pedido")]
        public DateTime? FechaUltimoPedido { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
