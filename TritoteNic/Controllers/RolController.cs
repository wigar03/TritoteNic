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
    public class RolController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<RolController> _logger;
        private readonly IMapper _mapper;

        public RolController(TritoteContext.TritoteConext context, ILogger<RolController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Roles");
                var roles = await _context.Roles.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<RolDto>>(roles));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Roles: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Roles.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RolDto>> GetRol(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Rol no válido: {id}");
                return BadRequest("ID de Rol no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Rol con ID: {id}");

                var rol = await _context.Roles.FindAsync(id);

                if (rol == null)
                {
                    _logger.LogWarning($"No se encontró ningún rol con ID: {id}");
                    return NotFound("Rol no encontrado.");
                }

                return Ok(_mapper.Map<RolDto>(rol));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener rol con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Rol.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RolDto>> PostRol(RolCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un rol nulo en la solicitud.");
                return BadRequest("El rol no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo rol con nombre: {createDto.NombreRol}");

                // Verificar si el rol ya existe
                var existing = await _context.Roles
                    .FirstOrDefaultAsync(r => r.NombreRol == createDto.NombreRol);
                if (existing != null)
                {
                    _logger.LogWarning($"El rol con nombre {createDto.NombreRol} ya existe.");
                    ModelState.AddModelError("NombreRol", "El nombre del rol ya está en uso.");
                    return BadRequest(ModelState);
                }

                // Verificar la validez del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de rol recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Crear el nuevo rol
                var rol = _mapper.Map<Rol>(createDto);

                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo rol '{createDto.NombreRol}' creado con ID: {rol.IdRol}");
                return CreatedAtAction(nameof(GetRol), new { id = rol.IdRol }, _mapper.Map<RolDto>(rol));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo rol: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo rol.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutRol(int id, RolUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdRol)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del rol no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando rol con ID: {id}");

                var rolExistente = await _context.Roles.FindAsync(id);
                if (rolExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún rol con ID: {id}");
                    return NotFound("El rol no existe.");
                }

                //Actualizar solo las propiedades necesarias del rol existente
                _mapper.Map(updateDto, rolExistente);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Rol con ID {id} actualizado correctamente.");

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RolExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún rol con ID: {id}");
                    return NotFound("El rol no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el rol con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el rol.");
                }
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRol(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando rol con ID: {id}");

                var rol = await _context.Roles.FindAsync(id);
                if (rol == null)
                {
                    _logger.LogWarning($"No se encontró ningún rol con ID: {id}");
                    return NotFound("Rol no encontrado.");
                }

                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Rol con ID {id} eliminado correctamente");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el rol con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el rol.");
            }
        }

        private bool RolExiste(int id) => _context.Roles.Any(r => r.IdRol == id);
    }
}
