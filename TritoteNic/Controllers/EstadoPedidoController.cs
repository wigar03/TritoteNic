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
    public class EstadoPedidoController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<EstadoPedidoController> _logger;
        private readonly IMapper _mapper;

        public EstadoPedidoController(TritoteContext.TritoteConext context, ILogger<EstadoPedidoController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EstadoPedidoDto>>> GetEstadosPedido()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Estados de Pedido");
                var estados = await _context.EstadosPedidos.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<EstadoPedidoDto>>(estados));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Estados de Pedido: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Estados de Pedido.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EstadoPedidoDto>> GetEstadoPedido(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Estado de Pedido no válido: {id}");
                return BadRequest("ID de Estado de Pedido no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Estado de Pedido con ID: {id}");
                var estado = await _context.EstadosPedidos.FindAsync(id);

                if (estado == null)
                {
                    _logger.LogWarning($"No se encontró ningún estado de pedido con ID: {id}");
                    return NotFound("Estado de Pedido no encontrado.");
                }

                return Ok(_mapper.Map<EstadoPedidoDto>(estado));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener estado de pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Estado de Pedido.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EstadoPedidoDto>> PostEstadoPedido(EstadoPedidoCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un estado de pedido nulo en la solicitud.");
                return BadRequest("El estado de pedido no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo estado de pedido con nombre: {createDto.NombreEstadoPedido}");

                // Verificar si el estado ya existe
                var existing = await _context.EstadosPedidos
                    .FirstOrDefaultAsync(e => e.NombreEstadoPedido == createDto.NombreEstadoPedido);
                if (existing != null)
                {
                    _logger.LogWarning($"El estado de pedido con nombre {createDto.NombreEstadoPedido} ya existe.");
                    ModelState.AddModelError("NombreEstadoPedido", "El nombre del estado de pedido ya está en uso.");
                    return BadRequest(ModelState);
                }

                // Verificar la validez del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de estado de pedido recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Crear el nuevo estado
                var estado = _mapper.Map<EstadoPedido>(createDto);
                _context.EstadosPedidos.Add(estado);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo estado de pedido '{createDto.NombreEstadoPedido}' creado con ID: {estado.IdEstadoPedido}");
                return CreatedAtAction(nameof(GetEstadoPedido), new { id = estado.IdEstadoPedido }, _mapper.Map<EstadoPedidoDto>(estado));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo estado de pedido: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo estado de pedido.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutEstadoPedido(int id, EstadoPedidoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdEstadoPedido)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del estado de pedido no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando estado de pedido con ID: {id}");

                var estadoExistente = await _context.EstadosPedidos.FindAsync(id);
                if (estadoExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún estado de pedido con ID: {id}");
                    return NotFound("El estado de pedido no existe.");
                }

                //Actualizar solo las propiedades necesarias
                _mapper.Map(updateDto, estadoExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estado de pedido con ID {id} actualizado correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!EstadoPedidoExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún estado de pedido con ID: {id}");
                    return NotFound("El estado de pedido no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el estado de pedido con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el estado de pedido.");
                }
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEstadoPedido(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando estado de pedido con ID: {id}");

                var estado = await _context.EstadosPedidos.FindAsync(id);
                if (estado == null)
                {
                    _logger.LogWarning($"No se encontró ningún estado de pedido con ID: {id}");
                    return NotFound("Estado de Pedido no encontrado.");
                }

                _context.EstadosPedidos.Remove(estado);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estado de pedido con ID {id} eliminado correctamente");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el estado de pedido con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el estado de pedido.");
            }
        }

        private bool EstadoPedidoExiste(int id) => _context.EstadosPedidos.Any(e => e.IdEstadoPedido == id);
    }
}
