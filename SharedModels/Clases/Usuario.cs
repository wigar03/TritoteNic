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
        public int IdUsuario { get; set; }

        [Required, StringLength(100)]
        public string NombreUsuario { get; set; }

        [Required, StringLength(100)]
        public string EmailUsuario { get; set; }

        [Required, StringLength(255)]
        public string ContrasenaUsuario { get; set; }

        [Required]
        [ForeignKey("Rol")]
        [Column("id_rol")]
        public int IdRol { get; set; }

        public Rol Rol { get; set; }

        [Required, StringLength(20)]
        public string EstadoUsuario { get; set; } // Activo / Inactivo

        [Column("fecha_creacion")]
        public DateTime FechaCreacionUsuario { get; set; } = DateTime.Now;

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

}
