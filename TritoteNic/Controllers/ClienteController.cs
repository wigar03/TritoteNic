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
    public class ClienteController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<ClienteController> _logger;
        private readonly IMapper _mapper;

        public ClienteController(TritoteContext.TritoteConext context, ILogger<ClienteController> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Clientes");
                var clientes = await _context.Clientes.ToListAsync();
                return Ok(_mapper.Map<IEnumerable<ClienteDto>>(clientes));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Clientes: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener los Clientes.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Cliente no válido: {id}");
                return BadRequest("ID de Cliente no válido.");
            }

            try
            {
                _logger.LogInformation($"Obteniendo Cliente con ID: {id}");
                var cliente = await _context.Clientes.FindAsync(id);

                if (cliente == null)
                {
                    _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                    return NotFound("Cliente no encontrado.");
                }

                return Ok(_mapper.Map<ClienteDto>(cliente));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener cliente con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Cliente.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClienteDto>> PostCliente(ClienteCreateDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogError("Se recibió un cliente nulo en la solicitud.");
                return BadRequest("El cliente no puede ser nulo.");
            }

            try
            {
                _logger.LogInformation($"Creando un nuevo cliente con nombre: {createDto.NombreCliente}");

                // Verificar duplicados (email opcional)
                if (!string.IsNullOrWhiteSpace(createDto.EmailCliente))
                {
                    var existingEmail = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.EmailCliente == createDto.EmailCliente);
                    if (existingEmail != null)
                    {
                        _logger.LogWarning($"El email {createDto.EmailCliente} ya existe.");
                        ModelState.AddModelError("EmailCliente", "El email ya está en uso.");
                        return BadRequest(ModelState);
                    }
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de cliente recibido no es válido.");
                    return BadRequest(ModelState);
                }

                var cliente = _mapper.Map<Cliente>(createDto);
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nuevo cliente '{createDto.NombreCliente}' creado con ID: {cliente.IdCliente}");
                return CreatedAtAction(nameof(GetCliente), new { id = cliente.IdCliente }, _mapper.Map<ClienteDto>(cliente));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear un nuevo cliente: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al crear un nuevo cliente.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCliente(int id, ClienteUpdateDto updateDto)
        {
            if (updateDto == null || id != updateDto.IdCliente)
            {
                return BadRequest("Los datos de entrada no son válidos o el ID del cliente no coincide");
            }

            try
            {
                _logger.LogInformation($"Actualizando cliente con ID: {id}");

                var clienteExistente = await _context.Clientes.FindAsync(id);
                if (clienteExistente == null)
                {
                    _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                    return NotFound("El cliente no existe.");
                }

                // Verificar conflicto de email si se actualiza
                if (!string.IsNullOrWhiteSpace(updateDto.EmailCliente))
                {
                    var emailConflict = await _context.Clientes
                        .AnyAsync(c => c.EmailCliente == updateDto.EmailCliente && c.IdCliente != id);
                    if (emailConflict)
                    {
                        ModelState.AddModelError("EmailCliente", "El email ya está en uso por otro cliente.");
                        return BadRequest(ModelState);
                    }
                }

                _mapper.Map(updateDto, clienteExistente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cliente con ID {id} actualizado correctamente.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ClienteExiste(id))
                {
                    _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                    return NotFound("El cliente no se encontró durante la actualización.");
                }
                else
                {
                    _logger.LogError($"Error de concurrencia al actualizar el cliente con ID: {id}. Detalles: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "Error interno del servidor al actualizar el cliente.");
                }
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                _logger.LogInformation($"Eliminando cliente con ID: {id}");

                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                    return NotFound("Cliente no encontrado.");
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cliente con ID {id} eliminado correctamente");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el cliente con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Se produjo un error al eliminar el cliente.");
            }
        }

        private bool ClienteExiste(int id) => _context.Clientes.Any(c => c.IdCliente == id);
    }
}
