using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using TritoteNic.Data;
using System.Linq;

namespace TritoteNic.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(TritoteContext.TritoteConext context, ILogger<PedidoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ValidarEntidadesRelacionadasAsync(PedidoCreateDto createDto)
        {
            // Validar que el cliente existe
            var cliente = await _context.Clientes.FindAsync(createDto.IdCliente);
            if (cliente == null)
                throw new InvalidOperationException($"El cliente con ID {createDto.IdCliente} no existe.");

            // Validar que el usuario existe
            var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == createDto.IdUsuario);
            if (!usuarioExiste)
                throw new InvalidOperationException($"El usuario con ID {createDto.IdUsuario} no existe.");

            // Validar que el estado de pedido existe
            var estadoExiste = await _context.EstadosPedidos.AnyAsync(e => e.IdEstadoPedido == createDto.IdEstadoPedido);
            if (!estadoExiste)
                throw new InvalidOperationException($"El estado de pedido con ID {createDto.IdEstadoPedido} no existe.");

            // Validar que el método de pago existe
            var metodoPagoExiste = await _context.MetodosPago.AnyAsync(m => m.IdMetodoPago == createDto.IdMetodoPago);
            if (!metodoPagoExiste)
                throw new InvalidOperationException($"El método de pago con ID {createDto.IdMetodoPago} no existe.");
        }

        public async Task ValidarDatosPedidoAsync(PedidoCreateDto createDto)
        {
            // Validar descuento
            if (createDto.Descuento < 0 || createDto.Descuento > 100)
                throw new ArgumentException("El descuento debe estar entre 0 y 100.");

            // Validar que tenga detalles
            if (createDto.Detalles == null || !createDto.Detalles.Any())
                throw new ArgumentException("El pedido debe tener al menos un detalle.");

            // Validar entidades relacionadas
            await ValidarEntidadesRelacionadasAsync(createDto);

            // Validar stock
            await ValidarStockAsync(createDto.Detalles);
        }

        public Task<decimal> CalcularSubtotalAsync(List<DetallePedidoCreateDto> detalles)
        {
            decimal subtotal = 0;

            foreach (var detalleDto in detalles)
            {
                decimal subtotalDetalle;
                if (detalleDto.SubtotalProducto > 0)
                {
                    subtotalDetalle = detalleDto.SubtotalProducto;
                }
                else
                {
                    subtotalDetalle = detalleDto.CantidadProducto * detalleDto.PrecioUnitarioProducto;
                }

                subtotal += subtotalDetalle;
            }

            return Task.FromResult(subtotal);
        }

        public Task<decimal> CalcularTotalAsync(decimal subtotal, decimal descuento)
        {
            var descuentoDecimal = descuento / 100m;
            var total = subtotal * (1 - descuentoDecimal);
            return Task.FromResult(total);
        }

        public async Task ValidarStockAsync(List<DetallePedidoCreateDto> detalles)
        {
            foreach (var detalleDto in detalles)
            {
                var producto = await _context.Productos.FindAsync(detalleDto.IdProducto);
                if (producto == null)
                    throw new InvalidOperationException($"El producto con ID {detalleDto.IdProducto} no existe.");

                if (producto.StockProducto < detalleDto.CantidadProducto)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto {producto.NombreProducto}. " +
                        $"Stock disponible: {producto.StockProducto}, solicitado: {detalleDto.CantidadProducto}");
                }
            }
        }

        public async Task ActualizarStockAsync(List<DetallePedido> detalles)
        {
            foreach (var detalle in detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.IdProducto);
                if (producto != null)
                {
                    producto.StockProducto -= detalle.CantidadProducto;
                    if (producto.StockProducto < 0)
                    {
                        producto.StockProducto = 0;
                    }

                    _logger.LogInformation(
                        $"Stock actualizado para producto {producto.NombreProducto}: " +
                        $"{producto.StockProducto + detalle.CantidadProducto} → {producto.StockProducto}");
                }
            }
        }

        public async Task ActualizarClienteAsync(int idCliente, decimal totalPedido)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                throw new InvalidOperationException($"Cliente con ID {idCliente} no encontrado.");

            // Actualizar total gastado y fecha último pedido
            cliente.TotalGastado += totalPedido;
            cliente.FechaUltimoPedido = DateTime.UtcNow;

            // Actualizar categoría según total gastado
            if (cliente.TotalGastado >= 70)
            {
                cliente.CategoriaCliente = "VIP";
            }
            else if (cliente.TotalGastado >= 10)
            {
                cliente.CategoriaCliente = "Frecuente";
            }
            else if (cliente.TotalGastado > 0)
            {
                cliente.CategoriaCliente = "Regular";
            }
            else
            {
                cliente.CategoriaCliente = null;
            }

            _logger.LogInformation(
                $"Cliente {cliente.NombreCliente} actualizado: " +
                $"TotalGastado=${cliente.TotalGastado}, Categoria={cliente.CategoriaCliente}");
        }

        public async Task<Pedido> CrearPedidoAsync(PedidoCreateDto createDto)
        {
            // Validar datos
            await ValidarDatosPedidoAsync(createDto);

            // Calcular subtotal
            var subtotalPedido = await CalcularSubtotalAsync(createDto.Detalles!);
            var subtotalFinal = createDto.SubtotalPedido ?? subtotalPedido;

            // Calcular total
            var totalPedido = createDto.TotalPedido > 0
                ? createDto.TotalPedido
                : await CalcularTotalAsync(subtotalFinal, createDto.Descuento);

            // Crear detalles
            var detallesAProcesar = new List<DetallePedido>();
            foreach (var detalleDto in createDto.Detalles!)
            {
                var subtotalDetalle = detalleDto.SubtotalProducto > 0
                    ? detalleDto.SubtotalProducto
                    : detalleDto.CantidadProducto * detalleDto.PrecioUnitarioProducto;

                detallesAProcesar.Add(new DetallePedido
                {
                    IdProducto = detalleDto.IdProducto,
                    CantidadProducto = detalleDto.CantidadProducto,
                    PrecioUnitarioProducto = detalleDto.PrecioUnitarioProducto,
                    SubtotalProducto = subtotalDetalle
                });
            }

            // Crear pedido
            var pedido = new Pedido
            {
                IdCliente = createDto.IdCliente,
                IdUsuario = createDto.IdUsuario,
                IdEstadoPedido = createDto.IdEstadoPedido,
                IdMetodoPago = createDto.IdMetodoPago,
                FechaPedido = DateTime.UtcNow,
                SubtotalPedido = subtotalFinal,
                Descuento = createDto.Descuento,
                TotalPedido = totalPedido,
                Detalles = detallesAProcesar
            };

            // ⚠️ IMPORTANTE: Usar transacción para asegurar que TODO se actualice de forma atómica
            // - El pedido solo se crea cuando se confirma
            // - El stock solo se actualiza cuando se confirma el pedido
            // - El cliente solo se actualiza cuando se confirma el pedido
            // - Las ventas se calculan dinámicamente desde los pedidos guardados en BD
            // Si algo falla, TODO se revierte (pedido no creado, stock no actualizado, cliente no actualizado)
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Guardar pedido en la BD (con todos sus detalles)
                    //    Esto hace que el pedido aparezca en las consultas de ventas
                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync();

                    // 2. Actualizar stock de productos (SOLO después de confirmar el pedido)
                    //    Se resta la cantidad vendida del stock disponible
                    await ActualizarStockAsync(detallesAProcesar);

                    // 3. Actualizar información del cliente (SOLO después de confirmar el pedido)
                    //    - TotalGastado: se suma el total del pedido
                    //    - FechaUltimoPedido: se actualiza a la fecha actual
                    //    - CategoriaCliente: se recalcula según el total gastado
                    await ActualizarClienteAsync(createDto.IdCliente, totalPedido);

                    // 4. Guardar todos los cambios (stock y cliente) en una sola operación
                    await _context.SaveChangesAsync();

                    // 5. Confirmar transacción - TODO se actualiza al mismo tiempo
                    //    Si llegamos aquí, significa que:
                    //    - ✅ El pedido está guardado
                    //    - ✅ El stock está actualizado
                    //    - ✅ El cliente está actualizado
                    //    - ✅ Las ventas se calcularán automáticamente desde los pedidos guardados
                    await transaction.CommitAsync();

                    _logger.LogInformation($"✅ Transacción completada exitosamente para pedido ID: {pedido.IdPedido}. Stock y cliente actualizados.");
                }
                catch (Exception ex)
                {
                    // Si algo falla, revertir TODOS los cambios
                    // - El pedido NO se crea
                    // - El stock NO se actualiza
                    // - El cliente NO se actualiza
                    // - Las ventas NO incluyen este pedido (porque no existe en BD)
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"❌ Error en transacción al crear pedido. TODOS los cambios fueron revertidos (pedido no creado, stock no actualizado, cliente no actualizado).");
                    throw;
                }
            }

            _logger.LogInformation($"Pedido creado exitosamente con ID: {pedido.IdPedido}");

            return pedido;
        }
    }
}
