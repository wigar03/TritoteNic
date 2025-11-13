using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TritoteNic.Data;
using SharedModels.Dto;
using SharedModels.Clases;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<UsuarioController> _logger;
        private readonly IMapper _mapper;

        public UsuarioController(TritoteContext.TritoteConext context, ILogger<UsuarioController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Usuarios");
                var usuarios = await _context.Usuarios.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<UsuarioDto>>(usuarios));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Usuarios: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Usuarios.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Usuario no válido: {id}");
                return BadRequest("ID de Usuario no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Usuario con ID: {id}");

                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    _logger.LogWarning($"No se encontró ningún usuario con ID: {id}");
                    return NotFound("Usuario no encontrado.");
                }
                return Ok(_mapper.Map<UsuarioDto>(usuario));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Usuario.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioDto>> PostUsuario(UsuarioCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un usuario nulo en la solicitud.");
                return BadRequest("El usuario no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo usuario con nombre: {createDto.NombreUsuario}");

                // Verificar si el usuario ya existe
                var existingUser = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.EmailUsuario == createDto.EmailUsuario);
                if (existingUser != null)
                {
                    _logger.LogWarning($"El usuario con email {createDto.EmailUsuario} ya existe.");
                    ModelState.AddModelError("EmailUsuario", "El email ya está en uso.");
                    return BadRequest(ModelState);
                }

                // Verificar la validez del modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de usuario recibido no es válido.");
                    return BadRequest(ModelState);
                }

                // Crear el nuevo usuario
                var nuevoUsuario = _mapper.Map<Usuario>(createDto);

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo usuario '{createDto.NombreUsuario}' creado con ID: {nuevoUsuario.IdUsuario}");
                return CreatedAtAction(nameof(GetUsuario), new { id = nuevoUsuario.IdUsuario }, _mapper.Map<UsuarioDto>(nuevoUsuario));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo usuario: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo usuario.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutUsuario(int id, UsuarioUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdUsuario)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del usuario no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando usuario con ID: {id}");

                var usuarioExistente = await _context.Usuarios.FindAsync(id);
                if (usuarioExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún usuario con ID: {id}");
                    return NotFound("El usuario no existe.");
                }

                //Actualizar solo las propiedades necesarias del usuario existente
                _mapper.Map(updateDto, usuarioExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario con ID {id} actualizado correctamente.");

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UsuarioExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún usuario con ID: {id}");
                    return NotFound("El usuario no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el usuario con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el usuario.");
                }
            }
        }

        private bool UsuarioExiste(int id)
        {
            return _context.Usuarios.Any(u => u.IdUsuario == id);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando usuario con ID: {id}");

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning($"No se encontró ningún usuario con ID: {id}");
                    return NotFound("Usuario no encontrado.");
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario con ID {id} elimiando correctamente");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el usuario con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el usuario.");
            }
        }
    }
}
