using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using TritoteNic.Data;
using System.Linq;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<PedidoController> _logger;
        private readonly IMapper _mapper;

        public PedidoController(TritoteContext.TritoteConext context, ILogger<PedidoController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidos()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Pedidos");
                var pedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Usuario)
                    .Include(p => p.EstadoPedido)
                    .Include(p => p.MetodoPago)
                    .Include(p => p.Detalles)
                        .ThenInclude(d => d.Producto)
                    .ToListAsync();

                var pedidosDto = _mapper.Map<IEnumerable<PedidoDto>>(pedidos);
                
                // Mapear información adicional
                foreach (var pedido in pedidos)
                {
                    var pedidoDto = pedidosDto.FirstOrDefault(p => p.IdPedido == pedido.IdPedido);
                    if (pedidoDto != null)
                    {
                        pedidoDto.NombreCliente = pedido.Cliente?.NombreCliente;
                        pedidoDto.NombreUsuario = pedido.Usuario?.NombreUsuario;
                        pedidoDto.NombreEstadoPedido = pedido.EstadoPedido?.NombreEstadoPedido;
                        pedidoDto.NombreMetodoPago = pedido.MetodoPago?.NombreMetodoPago;
                        
                        if (pedido.Detalles != null && pedido.Detalles.Any())
                        {
                            pedidoDto.Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
                            {
                                IdDetalle = d.IdDetalle,
                                IdPedido = d.IdPedido,
                                IdProducto = d.IdProducto,
                                NombreProducto = d.Producto?.NombreProducto,
                                CantidadProducto = d.CantidadProducto,
                                PrecioUnitarioProducto = d.PrecioUnitarioProducto,
                                SubtotalProducto = d.SubtotalProducto
                            }).ToList();
                        }
                    }
                }

                return Ok(pedidosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Pedidos: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Pedidos.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PedidoDto>> GetPedido(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Pedido no válido: {id}");
                return BadRequest("ID de Pedido no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Pedido con ID: {id}");
                var pedido = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Usuario)
                    .Include(p => p.EstadoPedido)
                    .Include(p => p.MetodoPago)
                    .Include(p => p.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(p => p.IdPedido == id);

                if (pedido == null)
                {
                    _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                    return NotFound("Pedido no encontrado.");
                }

                var pedidoDto = _mapper.Map<PedidoDto>(pedido);
                
                // Mapear información adicional
                pedidoDto.NombreCliente = pedido.Cliente?.NombreCliente;
                pedidoDto.NombreUsuario = pedido.Usuario?.NombreUsuario;
                pedidoDto.NombreEstadoPedido = pedido.EstadoPedido?.NombreEstadoPedido;
                pedidoDto.NombreMetodoPago = pedido.MetodoPago?.NombreMetodoPago;
                
                if (pedido.Detalles != null && pedido.Detalles.Any())
                {
                    pedidoDto.Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
                    {
                        IdDetalle = d.IdDetalle,
                        IdPedido = d.IdPedido,
                        IdProducto = d.IdProducto,
                        NombreProducto = d.Producto?.NombreProducto,
                        CantidadProducto = d.CantidadProducto,
                        PrecioUnitarioProducto = d.PrecioUnitarioProducto,
                        SubtotalProducto = d.SubtotalProducto
                    }).ToList();
                }

                return Ok(pedidoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Pedido.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PedidoDto>> PostPedido(PedidoCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un pedido nulo en la solicitud.");
                return BadRequest("El pedido no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo pedido para cliente ID: {createDto.IdCliente}");

                // Verificar la validez del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de pedido recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Validar descuento (0-100%)
                if (createDto.Descuento < 0 || createDto.Descuento > 100)
                {
                    ModelState.AddModelError("Descuento", "El descuento debe estar entre 0 y 100.");
                    return BadRequest(ModelState);
                }

                // Validar FKs
                var cliente = await _context.Clientes.FindAsync(createDto.IdCliente);
                if (cliente == null)
                {
                    ModelState.AddModelError("IdCliente", "El cliente no existe.");
                    return BadRequest(ModelState);
                }

                if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == createDto.IdUsuario))
                {
                    ModelState.AddModelError("IdUsuario", "El usuario no existe.");
                    return BadRequest(ModelState);
                }

                if (!await _context.EstadosPedidos.AnyAsync(e => e.IdEstadoPedido == createDto.IdEstadoPedido))
                {
                    ModelState.AddModelError("IdEstadoPedido", "El estado de pedido no existe.");
                    return BadRequest(ModelState);
                }

                if (!await _context.MetodosPago.AnyAsync(m => m.IdMetodoPago == createDto.IdMetodoPago))
                {
                    ModelState.AddModelError("IdMetodoPago", "El método de pago no existe.");
                    return BadRequest(ModelState);
                }

                // Validar y procesar detalles del pedido
                if (createDto.Detalles == null || !createDto.Detalles.Any())
                {
                    ModelState.AddModelError("Detalles", "El pedido debe tener al menos un detalle.");
                    return BadRequest(ModelState);
                }

                decimal subtotalPedido = 0;
                var detallesAProcesar = new List<DetallePedido>();

                // Validar stock y calcular subtotal
                foreach (var detalleDto in createDto.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalleDto.IdProducto);
                    if (producto == null)
                    {
                        ModelState.AddModelError("Detalles", $"El producto con ID {detalleDto.IdProducto} no existe.");
                        return BadRequest(ModelState);
                    }

                    // Validar stock
                    if (producto.StockProducto < detalleDto.CantidadProducto)
                    {
                        ModelState.AddModelError("Detalles", 
                            $"Stock insuficiente para el producto {producto.NombreProducto}. Stock disponible: {producto.StockProducto}, solicitado: {detalleDto.CantidadProducto}");
                        return BadRequest(ModelState);
                    }

                    // Calcular subtotal si no viene
                    decimal subtotalDetalle;
                    if (detalleDto.SubtotalProducto > 0)
                    {
                        subtotalDetalle = detalleDto.SubtotalProducto;
                    }
                    else
                    {
                        subtotalDetalle = detalleDto.CantidadProducto * detalleDto.PrecioUnitarioProducto;
                    }

                    subtotalPedido += subtotalDetalle;

                    var detalle = new DetallePedido
                    {
                        IdProducto = detalleDto.IdProducto,
                        CantidadProducto = detalleDto.CantidadProducto,
                        PrecioUnitarioProducto = detalleDto.PrecioUnitarioProducto,
                        SubtotalProducto = subtotalDetalle
                    };

                    detallesAProcesar.Add(detalle);
                }

                // Calcular SubtotalPedido si no viene
                if (createDto.SubtotalPedido == null || createDto.SubtotalPedido == 0)
                {
                    createDto.SubtotalPedido = subtotalPedido;
                }

                // Calcular TotalPedido aplicando descuento
                decimal totalPedido;
                if (createDto.TotalPedido > 0)
                {
                    totalPedido = createDto.TotalPedido;
                }
                else
                {
                    var subtotal = createDto.SubtotalPedido ?? subtotalPedido;
                    var descuento = createDto.Descuento / 100m;
                    totalPedido = subtotal * (1 - descuento);
                }

                // Crear el pedido
                var pedido = new Pedido
                {
                    IdCliente = createDto.IdCliente,
                    IdUsuario = createDto.IdUsuario,
                    IdEstadoPedido = createDto.IdEstadoPedido,
                    IdMetodoPago = createDto.IdMetodoPago,
                    FechaPedido = DateTime.Now,
                    SubtotalPedido = createDto.SubtotalPedido ?? subtotalPedido,
                    Descuento = createDto.Descuento,
                    TotalPedido = totalPedido,
                    Detalles = detallesAProcesar
                };

                // Actualizar stock de productos
                foreach (var detalle in detallesAProcesar)
                {
                    var producto = await _context.Productos.FindAsync(detalle.IdProducto);
                    if (producto != null)
                    {
                        producto.StockProducto -= detalle.CantidadProducto;
                        if (producto.StockProducto < 0)
                        {
                            producto.StockProducto = 0;
                        }
                    }
                }

                // Actualizar cliente
                cliente.TotalGastado += totalPedido;
                cliente.FechaUltimoPedido = DateTime.Now;

                // Actualizar categoría del cliente
                if (cliente.TotalGastado >= 100000)
                {
                    cliente.CategoriaCliente = "VIP";
                }
                else if (cliente.TotalGastado >= 50000)
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

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Cargar relaciones para el DTO
                await _context.Entry(pedido)
                    .Reference(p => p.Cliente).LoadAsync();
                await _context.Entry(pedido)
                    .Reference(p => p.Usuario).LoadAsync();
                await _context.Entry(pedido)
                    .Reference(p => p.EstadoPedido).LoadAsync();
                await _context.Entry(pedido)
                    .Reference(p => p.MetodoPago).LoadAsync();
                await _context.Entry(pedido)
                    .Collection(p => p.Detalles).LoadAsync();

                foreach (var detalle in pedido.Detalles)
                {
                    await _context.Entry(detalle)
                        .Reference(d => d.Producto).LoadAsync();
                }

                var pedidoDto = _mapper.Map<PedidoDto>(pedido);
                pedidoDto.NombreCliente = pedido.Cliente?.NombreCliente;
                pedidoDto.NombreUsuario = pedido.Usuario?.NombreUsuario;
                pedidoDto.NombreEstadoPedido = pedido.EstadoPedido?.NombreEstadoPedido;
                pedidoDto.NombreMetodoPago = pedido.MetodoPago?.NombreMetodoPago;
                
                if (pedido.Detalles != null && pedido.Detalles.Any())
                {
                    pedidoDto.Detalles = pedido.Detalles.Select(d => new DetallePedidoDto
                    {
                        IdDetalle = d.IdDetalle,
                        IdPedido = d.IdPedido,
                        IdProducto = d.IdProducto,
                        NombreProducto = d.Producto?.NombreProducto,
                        CantidadProducto = d.CantidadProducto,
                        PrecioUnitarioProducto = d.PrecioUnitarioProducto,
                        SubtotalProducto = d.SubtotalProducto
                    }).ToList();
                }

                _logger.LogInformation($"Nuevo pedido creado con ID: {pedido.IdPedido}");
                return CreatedAtAction(nameof(GetPedido), new { id = pedido.IdPedido }, pedidoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo pedido: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo pedido.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutPedido(int id, PedidoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdPedido)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del pedido no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando pedido con ID: {id}");

                var pedidoExistente = await _context.Pedidos.FindAsync(id);
                if (pedidoExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                    return NotFound("El pedido no existe.");
                }

                //Actualizar solo las propiedades necesarias del pedido existente
                _mapper.Map(updateDto, pedidoExistente);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Pedido con ID {id} actualizado correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!PedidoExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                    return NotFound("El pedido no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el pedido con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el pedido.");
                }
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePedido(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando pedido con ID: {id}");

                var pedido = await _context.Pedidos.FindAsync(id);
                if (pedido == null)
                {
                    _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                    return NotFound("Pedido no encontrado.");
                }

                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Pedido con ID {id} eliminado correctamente");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el pedido.");
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchPedido(int id, JsonPatchDocument<PedidoUpdateDto> patchDto)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Pedido no válido: {id}");
                return BadRequest("ID de Pedido no válido.");
            }

            try
            {
                _logger.LogInformation($"Aplicando el parche al pedido con ID: {id}");

                var pedido = await _context.Pedidos.FindAsync(id);
                if (pedido == null)
                {
                    _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                    return NotFound("El pedido no se encontró.");
                }

                var pedidoDto = _mapper.Map<PedidoUpdateDto>(pedido);
                patchDto.ApplyTo(pedidoDto, ModelState);

                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de pedido después de aplicar el parche no es válido.");
                    return BadRequest(ModelState);
                }

                _mapper.Map(pedidoDto, pedido);

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        _logger.LogInformation($"Parche aplicado correctamente al pedido con ID: {id}");
                        return NoContent();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        transaction.Rollback();
                        if (!PedidoExiste(id))
                        {
                            _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                            return NotFound("El pedido no se encontró durante la actualización.");
                        }
                        else
                        {
                            _logger.LogError($"Error de concurrencia al aplicar el parche al pedido con ID: {id}. Detalles: {ex.Message}");
                            return StatusCode(StatusCodes.Status500InternalServerError,
                                "Error interno del servidor al aplicar el parche al pedido.");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Error al aplicar el parche al pedido con ID {id}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error interno del servidor al aplicar el parche al pedido.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al aplicar el parche al pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al aplicar el parche al pedido.");
            }
        }

        private bool PedidoExiste(int id) => _context.Pedidos.Any(p => p.IdPedido == id);
    }
}
