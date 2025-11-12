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
        [Column("nombre")]
        public string Nombre { get; set; }

        [StringLength(15)]
        [Column("telefono")]
        public string Telefono { get; set; }

        [StringLength(255)]
        [Column("direccion")]
        public string Direccion { get; set; }

        [StringLength(100)]
        [Column("email")]
        public string Email { get; set; }

        // Relación con pedidos
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
