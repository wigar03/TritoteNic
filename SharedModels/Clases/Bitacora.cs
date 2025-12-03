using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Clases
{
    [Table("Bitacora")]
    public class Bitacora
    {
        [Key]
        [Column("id_bitacora")]
        public int IdBitacora { get; set; }

        [Required]
        [Column("fecha_accion")]
        public DateTime FechaAccion { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [StringLength(100)]
        [Column("nombre_usuario")]
        public string? NombreUsuario { get; set; }

        [Required]
        [StringLength(50)]
        [Column("tabla_afectada")]
        public string TablaAfectada { get; set; } = string.Empty; // Cliente, Pedido, Producto, etc.

        [Required]
        [StringLength(50)]
        [Column("accion")]
        public string Accion { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, PATCH

        [Column("id_registro")]
        public int? IdRegistro { get; set; } // ID del registro afectado

        [StringLength(100)]
        [Column("descripcion_registro")]
        public string? DescripcionRegistro { get; set; } // Nombre del cliente, producto, etc.

        [Column("datos_anteriores", TypeName = "text")]
        public string? DatosAnteriores { get; set; } // JSON con datos antes del cambio

        [Column("datos_nuevos", TypeName = "text")]
        public string? DatosNuevos { get; set; } // JSON con datos después del cambio

        [Column("observaciones", TypeName = "text")]
        public string? Observaciones { get; set; }

        [StringLength(100)]
        [Column("ip_address")]
        public string? IpAddress { get; set; }
    }
}

