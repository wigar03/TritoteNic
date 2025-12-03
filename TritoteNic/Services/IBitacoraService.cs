using Microsoft.AspNetCore.Http;
using SharedModels.Clases;
using System.Security.Claims;
using System.Text.Json;

namespace TritoteNic.Services
{
    public interface IBitacoraService
    {
        Task RegistrarCambioAsync(
            string tablaAfectada,
            string accion,
            int? idRegistro,
            string? descripcionRegistro,
            object? datosAnteriores = null,
            object? datosNuevos = null,
            string? observaciones = null);
    }

    public class BitacoraService : IBitacoraService
    {
        private readonly Data.TritoteContext.TritoteConext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BitacoraService> _logger;

        public BitacoraService(
            Data.TritoteContext.TritoteConext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BitacoraService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task RegistrarCambioAsync(
            string tablaAfectada,
            string accion,
            int? idRegistro,
            string? descripcionRegistro,
            object? datosAnteriores = null,
            object? datosNuevos = null,
            string? observaciones = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("No se puede registrar en bitácora: HttpContext es nulo");
                    return;
                }

                // Obtener información del usuario desde los claims del JWT
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var userNameClaim = httpContext.User.FindFirst(ClaimTypes.Name);
                
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idUsuario))
                {
                    _logger.LogWarning("No se puede registrar en bitácora: Usuario no autenticado");
                    return;
                }

                // Obtener IP del cliente
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() 
                    ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                    ?? "Desconocida";

                // Serializar datos anteriores y nuevos a JSON
                string? datosAnterioresJson = null;
                string? datosNuevosJson = null;

                if (datosAnteriores != null)
                {
                    datosAnterioresJson = JsonSerializer.Serialize(datosAnteriores, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                }

                if (datosNuevos != null)
                {
                    datosNuevosJson = JsonSerializer.Serialize(datosNuevos, new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                }

                var bitacora = new Bitacora
                {
                    FechaAccion = DateTime.UtcNow,
                    IdUsuario = idUsuario,
                    NombreUsuario = userNameClaim?.Value,
                    TablaAfectada = tablaAfectada,
                    Accion = accion,
                    IdRegistro = idRegistro,
                    DescripcionRegistro = descripcionRegistro,
                    DatosAnteriores = datosAnterioresJson,
                    DatosNuevos = datosNuevosJson,
                    Observaciones = observaciones,
                    IpAddress = ipAddress
                };

                _context.Bitacoras.Add(bitacora);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bitácora registrada: {accion} en {tablaAfectada} por usuario {idUsuario}");
            }
            catch (Exception ex)
            {
                // No lanzar excepción para no interrumpir el flujo principal
                _logger.LogError(ex, $"Error al registrar en bitácora: {ex.Message}");
            }
        }
    }
}

