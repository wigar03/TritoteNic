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
                var pedidos = await _context.Pedidos.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<PedidoDto>>(pedidos));
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
                var pedido = await _context.Pedidos.FindAsync(id);

                if (pedido == null)
                {
                    _logger.LogWarning($"No se encontró ningún pedido con ID: {id}");
                    return NotFound("Pedido no encontrado.");
                }

                return Ok(_mapper.Map<PedidoDto>(pedido));
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

                // Opcional: Validaciones de FKs (evita errores de integridad)
                var fkMissing = false;
                if (!await _context.Clientes.AnyAsync(c => c.IdCliente == createDto.IdCliente))
                {
                    ModelState.AddModelError("IdCliente", "El cliente no existe.");
                    fkMissing = true;
                }
                if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == createDto.IdUsuario))
                {
                    ModelState.AddModelError("IdUsuario", "El usuario no existe.");
                    fkMissing = true;
                }
                if (!await _context.EstadosPedidos.AnyAsync(e => e.IdEstadoPedido == createDto.IdEstadoPedido))
                {
                    ModelState.AddModelError("IdEstadoPedido", "El estado de pedido no existe.");
                    fkMissing = true;
                }
                if (!await _context.MetodosPago.AnyAsync(m => m.IdMetodoPago == createDto.IdMetodoPago))
                {
                    ModelState.AddModelError("IdMetodoPago", "El método de pago no existe.");
                    fkMissing = true;
                }
                if (fkMissing) return BadRequest(ModelState);

                // Crear el nuevo pedido
                var pedido = _mapper.Map<Pedido>(createDto);

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo pedido creado con ID: {pedido.IdPedido}");
                return CreatedAtAction(nameof(GetPedido), new { id = pedido.IdPedido }, _mapper.Map<PedidoDto>(pedido));
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

        private bool PedidoExiste(int id) => _context.Pedidos.Any(p => p.IdPedido == id);
    }
}
