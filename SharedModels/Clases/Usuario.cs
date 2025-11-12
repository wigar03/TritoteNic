using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Clases
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int Id_Usuario { get; set; }

        [Required, StringLength(100)]
        [Column("nombre")]
        public string Nombre_Usuario { get; set; }

        [Required, StringLength(100)]
        [Column("email")]
        public string Email { get; set; }

        [Required, StringLength(255)]
        [Column("contraseña")]
        public string Contraseña { get; set; }

        [Required, StringLength(50)]
        [Column("rol")]
        public string Rol { get; set; } // Administrador, Vendedor, Analista

        [Required, StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } // Activo, Inactivo

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con pedidos (1 Usuario puede registrar muchos pedidos)
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

}
