using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using TritoteNic.Data;
using System.Linq;

namespace TritoteNic.Services
{
    public class ClienteService : IClienteService
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(TritoteContext.TritoteConext context, ILogger<ClienteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<string?> DeterminarCategoriaClienteAsync(decimal totalGastado)
        {
            string? categoria = null;

            if (totalGastado >= 70)
            {
                categoria = "VIP";
            }
            else if (totalGastado >= 10)
            {
                categoria = "Frecuente";
            }
            else if (totalGastado > 0)
            {
                categoria = "Regular";
            }

            return Task.FromResult(categoria);
        }

        public async Task CalcularMetricasClienteAsync(Cliente cliente)
        {
            try
            {
                // Cargar pedidos si no están cargados
                // Si la colección está vacía o es null, cargar desde BD
                if (cliente.Pedidos == null || !cliente.Pedidos.Any())
                {
                    var pedidos = await _context.Pedidos
                        .Where(p => p.IdCliente == cliente.IdCliente)
                        .ToListAsync();
                    cliente.Pedidos = pedidos;
                }

                // Calcular TotalGastado
                cliente.TotalGastado = cliente.Pedidos?.Sum(p => p.TotalPedido) ?? 0;

                // Calcular FechaUltimoPedido
                if (cliente.Pedidos != null && cliente.Pedidos.Any())
                {
                    cliente.FechaUltimoPedido = cliente.Pedidos.Max(p => p.FechaPedido);
                }

                // Determinar categoría
                cliente.CategoriaCliente = await DeterminarCategoriaClienteAsync(cliente.TotalGastado);

                _logger.LogInformation(
                    $"Métricas calculadas para cliente {cliente.NombreCliente}: " +
                    $"TotalGastado=${cliente.TotalGastado}, Categoria={cliente.CategoriaCliente}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al calcular métricas para cliente {cliente.IdCliente}: {ex.Message}");
                throw;
            }
        }

        public async Task CalcularMetricasTodosClientesAsync()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Pedidos)
                .ToListAsync();

            foreach (var cliente in clientes)
            {
                await CalcularMetricasClienteAsync(cliente);
            }

            await _context.SaveChangesAsync();
        }
    }
}
