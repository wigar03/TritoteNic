using SharedModels.Clases;

namespace TritoteNic.Services
{
    public interface IClienteService
    {
        Task CalcularMetricasClienteAsync(Cliente cliente);
        Task CalcularMetricasTodosClientesAsync();
        Task<string?> DeterminarCategoriaClienteAsync(decimal totalGastado);
    }
}
