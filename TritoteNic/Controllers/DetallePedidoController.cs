using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Clases;
using SharedModels.Dto;
using TritoteNic.Data;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallePedidoController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<DetallePedidoController> _logger;
        private readonly IMapper _mapper;

        public DetallePedidoController(TritoteContext.TritoteConext context, ILogger<DetallePedidoController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // GET: api/DetallePedido
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DetallePedidoDto>>> GetDetallesPedido()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Detalles de Pedido");
                var detalles = await _context.DetallesPedido.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<DetallePedidoDto>>(detalles));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Detalles de Pedido: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Detalles de Pedido.");
            }
        }

        // GET: api/DetallePedido/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetallePedidoDto>> GetDetallePedido(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Detalle de Pedido no válido: {id}");
                return BadRequest("ID de Detalle de Pedido no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Detalle de Pedido con ID: {id}");
                var detalle = await _context.DetallesPedido.FindAsync(id);

                if (detalle == null)
                {
                    _logger.LogWarning($"No se encontró ningún detalle de pedido con ID: {id}");
                    return NotFound("Detalle de Pedido no encontrado.");
                }

                return Ok(_mapper.Map<DetallePedidoDto>(detalle));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener detalle de pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Detalle de Pedido.");
            }
        }

        // GET: api/DetallePedido/pedido/{pedidoId}
        [HttpGet("pedido/{pedidoId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DetallePedidoDto>>> GetDetallesPorPedido(int pedidoId)
        {
            if (pedidoId <= 0)
            {
                return BadRequest("ID de Pedido no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo detalles del pedido ID: {pedidoId}");
                var detalles = await _context.DetallesPedido
                    .Where(d => d.IdPedido == pedidoId)
                    .ToListAsync();

                return Ok(_mapper.Map<IEnumerable<DetallePedidoDto>>(detalles));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener detalles del pedido {pedidoId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los detalles del pedido.");
            }
        }

        // POST: api/DetallePedido
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DetallePedidoDto>> PostDetallePedido(DetallePedidoCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un detalle de pedido nulo en la solicitud.");
                return BadRequest("El detalle de pedido no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando detalle para Pedido ID: {createDto.IdPedido}, Producto ID: {createDto.IdProducto}");

                // Validar FKs (opcional pero evita errores de integridad)
                var fkInvalid = false;
                if (!await _context.Pedidos.AnyAsync(p => p.IdPedido == createDto.IdPedido))
                {
                    ModelState.AddModelError("IdPedido", "El pedido no existe.");
                    fkInvalid = true;
                }
                if (!await _context.Productos.AnyAsync(p => p.IdProducto == createDto.IdProducto))
                {
                    ModelState.AddModelError("IdProducto", "El producto no existe.");
                    fkInvalid = true;
                }
                if (fkInvalid)
                {
                    _logger.LogWarning("FKs inválidas al crear detalle de pedido.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de detalle recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Mapeo manual porque los nombres del CreateDto no coinciden con la entidad
                var detalle = new DetallePedido
                {
                    IdPedido = createDto.IdPedido,
                    IdProducto = createDto.IdProducto,
                    CantidadProducto = createDto.Cantidad,
                    PrecioUnitarioProducto = createDto.PrecioUnitario,
                    SubtotalProducto = createDto.Subtotal
                };

                _context.DetallesPedido.Add(detalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Detalle creado con ID: {detalle.IdDetalle}");
                return CreatedAtAction(nameof(GetDetallePedido), new { id = detalle.IdDetalle }, _mapper.Map<DetallePedidoDto>(detalle));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear detalle de pedido: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear el detalle de pedido.");
            }
        }

        // PUT: api/DetallePedido/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutDetallePedido(int id, DetallePedidoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdDetalle)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del detalle no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando detalle de pedido con ID: {id}");

                var detalleExistente = await _context.DetallesPedido.FindAsync(id);
                if (detalleExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún detalle de pedido con ID: {id}");
                    return NotFound("El detalle de pedido no existe.");
                }

                // Actualizar propiedades
                detalleExistente.IdPedido = updateDto.IdPedido;
                detalleExistente.IdProducto = updateDto.IdProducto;
                detalleExistente.CantidadProducto = updateDto.CantidadProducto;
                detalleExistente.PrecioUnitarioProducto = updateDto.PrecioUnitarioProducto;
                detalleExistente.SubtotalProducto = updateDto.SubtotalProducto;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Detalle de pedido con ID {id} actualizado correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DetallePedidoExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún detalle de pedido con ID: {id}");
                    return NotFound("El detalle de pedido no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el detalle con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el detalle de pedido.");
                }
            }
        }

        // DELETE: api/DetallePedido/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDetallePedido(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando detalle de pedido con ID: {id}");

                var detalle = await _context.DetallesPedido.FindAsync(id);
                if (detalle == null)
                {
                    _logger.LogWarning($"No se encontró ningún detalle de pedido con ID: {id}");
                    return NotFound("Detalle de Pedido no encontrado.");
                }

                _context.DetallesPedido.Remove(detalle);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Detalle de pedido con ID {id} eliminado correctamente");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el detalle de pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el detalle de pedido.");
            }
        }

        private bool DetallePedidoExiste(int id) => _context.DetallesPedido.Any(d => d.IdDetalle == id);
    }
}
