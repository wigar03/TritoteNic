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
    public class MetodoPagoController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<MetodoPagoController> _logger;
        private readonly IMapper _mapper;

        public MetodoPagoController(TritoteContext.TritoteConext context, ILogger<MetodoPagoController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MetodoPagoDto>>> GetMetodosPago()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Métodos de Pago");
                var metodos = await _context.MetodosPago.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<MetodoPagoDto>>(metodos));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Métodos de Pago: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Métodos de Pago.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MetodoPagoDto>> GetMetodoPago(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Método de Pago no válido: {id}");
                return BadRequest("ID de Método de Pago no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Método de Pago con ID: {id}");
                var metodo = await _context.MetodosPago.FindAsync(id);

                if (metodo == null)
                {
                    _logger.LogWarning($"No se encontró ningún método de pago con ID: {id}");
                    return NotFound("Método de Pago no encontrado.");
                }

                return Ok(_mapper.Map<MetodoPagoDto>(metodo));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener método de pago con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Método de Pago.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MetodoPagoDto>> PostMetodoPago(MetodoPagoCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un método de pago nulo en la solicitud.");
                return BadRequest("El método de pago no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo método de pago con nombre: {createDto.NombreMetodoPago}");

                // Verificar si el método de pago ya existe
                var existing = await _context.MetodosPago
                    .FirstOrDefaultAsync(m => m.NombreMetodoPago == createDto.NombreMetodoPago);
                if (existing != null)
                {
                    _logger.LogWarning($"El método de pago con nombre {createDto.NombreMetodoPago} ya existe.");
                    ModelState.AddModelError("NombreMetodoPago", "El nombre del método de pago ya está en uso.");
                    return BadRequest(ModelState);
                }

                // Verificar la validez del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de método de pago recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Crear el nuevo método
                var metodo = _mapper.Map<MetodoPago>(createDto);
                _context.MetodosPago.Add(metodo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo método de pago '{createDto.NombreMetodoPago}' creado con ID: {metodo.IdMetodoPago}");
                return CreatedAtAction(nameof(GetMetodoPago), new { id = metodo.IdMetodoPago }, _mapper.Map<MetodoPagoDto>(metodo));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo método de pago: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo método de pago.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutMetodoPago(int id, MetodoPagoUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdMetodoPago)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del método de pago no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando método de pago con ID: {id}");

                var metodoExistente = await _context.MetodosPago.FindAsync(id);
                if (metodoExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún método de pago con ID: {id}");
                    return NotFound("El método de pago no existe.");
                }

                //Actualizar solo las propiedades necesarias del método existente
                _mapper.Map(updateDto, metodoExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Método de pago con ID {id} actualizado correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!MetodoPagoExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún método de pago con ID: {id}");
                    return NotFound("El método de pago no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el método de pago con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el método de pago.");
                }
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMetodoPago(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando método de pago con ID: {id}");

                var metodo = await _context.MetodosPago.FindAsync(id);
                if (metodo == null)
                {
                    _logger.LogWarning($"No se encontró ningún método de pago con ID: {id}");
                    return NotFound("Método de Pago no encontrado.");
                }

                _context.MetodosPago.Remove(metodo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Método de pago con ID {id} eliminado correctamente");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el método de pago con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el método de pago.");
            }
        }

        private bool MetodoPagoExiste(int id) => _context.MetodosPago.Any(m => m.IdMetodoPago == id);
    }
}
