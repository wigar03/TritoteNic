using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using TritoteNic.Data;
using TritoteNic.Services;
using System.Linq;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOrVendedor")]
    public class PedidoController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<PedidoController> _logger;
        private readonly IMapper _mapper;
        private readonly IPedidoService _pedidoService;
        private readonly Services.IBitacoraService _bitacoraService;

        public PedidoController(
            TritoteContext.TritoteConext context, 
            ILogger<PedidoController> logger, 
            IMapper mapper,
            IPedidoService pedidoService,
            Services.IBitacoraService bitacoraService)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _pedidoService = pedidoService;
            _bitacoraService = bitacoraService;
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

            // Verificar la validez del modelo
            if (!ModelState.IsValid)
            {
                _logger.LogError("El modelo de pedido recibido no es válido.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo pedido para cliente ID: {createDto.IdCliente}");

                // ✅ Usar Service para crear pedido (toda la lógica de negocio está aquí)
                var pedido = await _pedidoService.CrearPedidoAsync(createDto);

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

                // Mapear a DTO
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

                // Registrar en bitácora
                await _bitacoraService.RegistrarCambioAsync(
                    tablaAfectada: "Pedido",
                    accion: "CREATE",
                    idRegistro: pedido.IdPedido,
                    descripcionRegistro: $"Pedido #{pedido.IdPedido} - Cliente: {pedido.Cliente?.NombreCliente}",
                    datosAnteriores: null,
                    datosNuevos: createDto,
                    observaciones: $"Pedido creado - Total: {pedido.TotalPedido:C}");

                _logger.LogInformation($"Nuevo pedido creado con ID: {pedido.IdPedido}");
                return CreatedAtAction(nameof(GetPedido), new { id = pedido.IdPedido }, pedidoDto);
            }
            catch (ArgumentException argEx)
            {
                ModelState.AddModelError("", argEx.Message);
                return BadRequest(ModelState);
            }
            catch (InvalidOperationException invOpEx)
            {
                ModelState.AddModelError("", invOpEx.Message);
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo pedido: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo pedido.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
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

                // Guardar datos anteriores para bitácora
                var datosAnteriores = new
                {
                    pedidoExistente.IdEstadoPedido,
                    pedidoExistente.TotalPedido,
                    pedidoExistente.SubtotalPedido,
                    pedidoExistente.Descuento
                };

                //Actualizar solo las propiedades necesarias del pedido existente
                _mapper.Map(updateDto, pedidoExistente);

                await _context.SaveChangesAsync();

                // Registrar en bitácora
                await _bitacoraService.RegistrarCambioAsync(
                    tablaAfectada: "Pedido",
                    accion: "UPDATE",
                    idRegistro: id,
                    descripcionRegistro: $"Pedido #{id}",
                    datosAnteriores: datosAnteriores,
                    datosNuevos: updateDto,
                    observaciones: $"Pedido actualizado");

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
        [Authorize(Policy = "AdminOnly")]
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

                // Guardar datos antes de eliminar para bitácora
                var datosEliminados = new
                {
                    pedido.IdCliente,
                    pedido.TotalPedido,
                    pedido.FechaPedido
                };

                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();

                // Registrar en bitácora
                await _bitacoraService.RegistrarCambioAsync(
                    tablaAfectada: "Pedido",
                    accion: "DELETE",
                    idRegistro: id,
                    descripcionRegistro: $"Pedido #{id}",
                    datosAnteriores: datosEliminados,
                    datosNuevos: null,
                    observaciones: $"Pedido eliminado - Total: {pedido.TotalPedido:C}");

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
        [Authorize(Policy = "AdminOnly")]
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

                // Guardar datos anteriores para bitácora
                var datosAnteriores = new
                {
                    pedido.IdEstadoPedido,
                    pedido.TotalPedido,
                    pedido.SubtotalPedido,
                    pedido.Descuento
                };

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
                        
                        // Registrar en bitácora
                        await _bitacoraService.RegistrarCambioAsync(
                            tablaAfectada: "Pedido",
                            accion: "PATCH",
                            idRegistro: id,
                            descripcionRegistro: $"Pedido #{id}",
                            datosAnteriores: datosAnteriores,
                            datosNuevos: pedidoDto,
                            observaciones: $"Pedido actualizado parcialmente");
                        
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
