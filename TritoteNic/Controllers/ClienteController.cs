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
    public class ClienteController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<ClienteController> _logger;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;

        public ClienteController(
            TritoteContext.TritoteConext context, 
            ILogger<ClienteController> logger, 
            IMapper mapper,
            IClienteService clienteService)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _clienteService = clienteService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            try
            {
                _logger.LogInformation("Obteniendo los Clientes");
                
                List<Cliente> clientes;
                try
                {
                    clientes = await _context.Clientes
                        .Include(c => c.Pedidos)
                        .ToListAsync();
                    _logger.LogInformation($"Se encontraron {clientes.Count} clientes");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError($"Error al consultar la base de datos: {dbEx.Message}");
                    _logger.LogError($"Stack trace: {dbEx.StackTrace}");
                    throw;
                }

                IEnumerable<ClienteDto> clientesDto;
                try
                {
                    clientesDto = _mapper.Map<IEnumerable<ClienteDto>>(clientes);
                    _logger.LogInformation("Mapeo de DTOs completado");
                }
                catch (Exception mapEx)
                {
                    _logger.LogError($"Error al mapear a DTOs: {mapEx.Message}");
                    throw;
                }

                // ✅ Calcular métricas directamente (no modificar entidades en operación de lectura)
                foreach (var cliente in clientes)
                {
                    var clienteDto = clientesDto.FirstOrDefault(c => c.IdCliente == cliente.IdCliente);
                    if (clienteDto != null)
                    {
                        try
                        {
                            // Calcular desde los pedidos cargados
                            if (cliente.Pedidos != null && cliente.Pedidos.Any())
                            {
                                clienteDto.TotalGastado = cliente.Pedidos.Sum(p => p.TotalPedido);
                                clienteDto.TotalPedidos = cliente.Pedidos.Count;
                                clienteDto.FechaUltimoPedido = cliente.Pedidos.Max(p => p.FechaPedido);
                            }
                            else
                            {
                                clienteDto.TotalGastado = 0;
                                clienteDto.TotalPedidos = 0;
                                clienteDto.FechaUltimoPedido = null;
                            }
                            
                            // Determinar categoría usando el service (sin modificar la entidad)
                            clienteDto.CategoriaCliente = await _clienteService.DeterminarCategoriaClienteAsync(clienteDto.TotalGastado);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al calcular métricas para cliente {cliente.IdCliente}: {ex.Message}");
                            // Valores por defecto si hay error
                            clienteDto.TotalGastado = 0;
                            clienteDto.TotalPedidos = 0;
                            clienteDto.FechaUltimoPedido = null;
                        }
                    }
                }

                return Ok(clientesDto);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError($"Error de base de datos al obtener los Clientes: {dbEx.Message}");
                _logger.LogError($"Inner exception: {dbEx.InnerException?.Message}");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Error de conexión a la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                _logger.LogError($"Error de red al conectar con la base de datos: {socketEx.Message}");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Error de conexión de red. Verifique su conexión a internet y que el servidor de base de datos esté accesible.");
            }
            catch (Npgsql.NpgsqlException npgsqlEx)
            {
                _logger.LogError($"Error de PostgreSQL al obtener los Clientes: {npgsqlEx.Message}");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    $"Error de conexión a PostgreSQL: {npgsqlEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los Clientes: {ex.Message}");
                _logger.LogError($"Tipo de excepción: {ex.GetType().FullName}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        $"Error interno del servidor: {ex.InnerException.Message}");
                }
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error interno del servidor al obtener los Clientes: {ex.Message}");
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
                var cliente = await _context.Clientes
                    .Include(c => c.Pedidos)
                    .FirstOrDefaultAsync(c => c.IdCliente == id);

                if (cliente == null)
                {
                    _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                    return NotFound("Cliente no encontrado.");
                }

                var clienteDto = _mapper.Map<ClienteDto>(cliente);
                
                // ✅ Calcular métricas directamente (no modificar entidad en operación de lectura)
                try
                {
                    if (cliente.Pedidos != null && cliente.Pedidos.Any())
                    {
                        clienteDto.TotalGastado = cliente.Pedidos.Sum(p => p.TotalPedido);
                        clienteDto.TotalPedidos = cliente.Pedidos.Count;
                        clienteDto.FechaUltimoPedido = cliente.Pedidos.Max(p => p.FechaPedido);
                    }
                    else
                    {
                        clienteDto.TotalGastado = 0;
                        clienteDto.TotalPedidos = 0;
                        clienteDto.FechaUltimoPedido = null;
                    }
                    
                    // Determinar categoría usando el service (sin modificar la entidad)
                    clienteDto.CategoriaCliente = await _clienteService.DeterminarCategoriaClienteAsync(clienteDto.TotalGastado);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error al calcular métricas para cliente {cliente.IdCliente}: {ex.Message}");
                    // Valores por defecto si hay error
                    clienteDto.TotalGastado = 0;
                    clienteDto.TotalPedidos = 0;
                    clienteDto.FechaUltimoPedido = null;
                }

                return Ok(clienteDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener cliente con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al obtener el Cliente.");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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

        [HttpPatch("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchCliente(int id, JsonPatchDocument<ClienteUpdateDto> patchDto)
        {
            if (id <= 0)
            {
                _logger.LogError($"ID de Cliente no válido: {id}");
                return BadRequest("ID de Cliente no válido.");
            }

            try
            {
                _logger.LogInformation($"Aplicando el parche al cliente con ID: {id}");

                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                    return NotFound("El cliente no se encontró.");
                }

                var clienteDto = _mapper.Map<ClienteUpdateDto>(cliente);
                patchDto.ApplyTo(clienteDto, ModelState);

                if (!ModelState.IsValid)
                {
                    _logger.LogError("El modelo de cliente después de aplicar el parche no es válido.");
                    return BadRequest(ModelState);
                }

                // Validar email único si se está actualizando
                if (!string.IsNullOrWhiteSpace(clienteDto.EmailCliente))
                {
                    var emailConflict = await _context.Clientes
                        .AnyAsync(c => c.EmailCliente == clienteDto.EmailCliente && c.IdCliente != id);
                    if (emailConflict)
                    {
                        ModelState.AddModelError("EmailCliente", "El email ya está en uso por otro cliente.");
                        return BadRequest(ModelState);
                    }
                }

                _mapper.Map(clienteDto, cliente);

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        _logger.LogInformation($"Parche aplicado correctamente al cliente con ID: {id}");
                        return NoContent();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        transaction.Rollback();
                        if (!ClienteExiste(id))
                        {
                            _logger.LogWarning($"No se encontró ningún cliente con ID: {id}");
                            return NotFound("El cliente no se encontró durante la actualización.");
                        }
                        else
                        {
                            _logger.LogError($"Error de concurrencia al aplicar el parche al cliente con ID: {id}. Detalles: {ex.Message}");
                            return StatusCode(StatusCodes.Status500InternalServerError,
                                "Error interno del servidor al aplicar el parche al cliente.");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"Error al aplicar el parche al cliente con ID {id}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error interno del servidor al aplicar el parche al cliente.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al aplicar el parche al cliente con ID {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error interno del servidor al aplicar el parche al cliente.");
            }
        }

        private bool ClienteExiste(int id) => _context.Clientes.Any(c => c.IdCliente == id);
    }
}
