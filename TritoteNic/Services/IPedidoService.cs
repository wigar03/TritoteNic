using SharedModels.Clases;
using SharedModels.Dto;

namespace TritoteNic.Services
{
    public interface IPedidoService
    {
        Task<Pedido> CrearPedidoAsync(PedidoCreateDto createDto);
        Task ValidarDatosPedidoAsync(PedidoCreateDto createDto);
        Task<decimal> CalcularSubtotalAsync(List<DetallePedidoCreateDto> detalles);
        Task<decimal> CalcularTotalAsync(decimal subtotal, decimal descuento);
        Task ValidarStockAsync(List<DetallePedidoCreateDto> detalles);
        Task ActualizarStockAsync(List<DetallePedido> detalles);
        Task ActualizarClienteAsync(int idCliente, decimal totalPedido);
        Task ValidarEntidadesRelacionadasAsync(PedidoCreateDto createDto);
    }
}
