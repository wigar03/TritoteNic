using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;


namespace TritoteNic.Data
{
    public class TritoteContext
    {
        public class TritoteConext : DbContext
        {
            public TritoteConext(DbContextOptions<TritoteConext> options) : base(options)
            {
            }
            public DbSet<Cliente> Clientes { get; set; }
            public DbSet<Producto> Productos { get; set; }
            public DbSet<Pedido> Pedidos { get; set; }
            public DbSet<DetallePedido> DetallesPedido { get; set; }
            public DbSet<Usuario> Usuarios { get; set; }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                // Configuraciones adicionales si es necesario
            }
        }
    }
}
