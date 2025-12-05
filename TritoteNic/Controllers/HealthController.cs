using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TritoteNic.Data;
using Npgsql;

namespace TritoteNic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Permitir acceso sin autenticación para verificar el estado de la API
    public class HealthController : ControllerBase
    {
        private readonly TritoteContext.TritoteConext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(TritoteContext.TritoteConext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("db")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Intentar una consulta simple a la base de datos
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Hacer una consulta simple para verificar que funciona
                    var clienteCount = await _context.Clientes.CountAsync();
                    
                    return Ok(new
                    {
                        status = "healthy",
                        database = "connected",
                        message = "Conexión a la base de datos exitosa",
                        clientesCount = clienteCount,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    // Si CanConnectAsync retorna false, intentar una operación que lance excepción
                    // para obtener más detalles del error
                    try
                    {
                        await _context.Clientes.CountAsync();
                        // Si llegamos aquí, la conexión funcionó pero CanConnectAsync dijo que no
                        return StatusCode(503, new
                        {
                            status = "unhealthy",
                            database = "disconnected",
                            message = "CanConnectAsync retornó false pero la consulta funcionó"
                        });
                    }
                    catch (Exception)
                    {
                        // Esta excepción será capturada por los catch blocks más abajo
                        throw;
                    }
                }
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                // Verificar si es un error de DNS específicamente
                if (socketEx.SocketErrorCode == System.Net.Sockets.SocketError.HostNotFound)
                {
                    _logger.LogError(socketEx, "Error de DNS al resolver el hostname");
                    
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        database = "dns_error",
                        message = "Error al resolver el hostname de la base de datos",
                        error = socketEx.Message,
                        suggestion = "Verifique su conexión a internet y la configuración de DNS. Intente cambiar a DNS público (8.8.8.8 o 1.1.1.1)"
                    });
                }
                
                // Otros errores de red
                _logger.LogError(socketEx, "Error de red al conectar con la base de datos");
                
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "network_error",
                    message = "Error de conexión de red",
                    error = socketEx.Message,
                    errorCode = socketEx.SocketErrorCode.ToString(),
                    suggestion = "Verifique su conexión a internet y que el firewall no esté bloqueando la conexión a Supabase"
                });
            }
            catch (Npgsql.NpgsqlException npgsqlEx)
            {
                _logger.LogError(npgsqlEx, "Error de PostgreSQL");
                
                // Errores específicos
                if (npgsqlEx.Message.Contains("password authentication failed"))
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        database = "authentication_error",
                        message = "Error de autenticación",
                        error = "La contraseña o el usuario son incorrectos",
                        suggestion = "Verifique las credenciales en appsettings.json"
                    });
                }
                else if (npgsqlEx.Message.Contains("could not translate host name") || 
                         npgsqlEx.Message.Contains("No such host is known"))
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        database = "hostname_error",
                        message = "No se puede resolver el hostname",
                        error = npgsqlEx.Message,
                        suggestion = "Verifique el hostname en appsettings.json y su conexión a internet"
                    });
                }

                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "postgresql_error",
                    message = "Error al conectar con PostgreSQL",
                    error = npgsqlEx.Message,
                    sqlState = npgsqlEx.SqlState,
                    errorType = npgsqlEx.GetType().Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la conexión a la base de datos");
                
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    database = "error",
                    message = $"Error de conexión: {ex.Message}",
                    innerException = ex.InnerException?.Message,
                    errorType = ex.GetType().FullName,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}

