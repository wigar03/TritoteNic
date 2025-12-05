using Microsoft.EntityFrameworkCore;
using TritoteNic.Data;
using System.Security.Claims;

namespace TritoteNic.Middleware
{
    public class UserStatusValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserStatusValidationMiddleware> _logger;

        public UserStatusValidationMiddleware(RequestDelegate next, ILogger<UserStatusValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, TritoteContext.TritoteConext dbContext)
        {
            // Solo validar si el usuario está autenticado y no es una ruta de login o inicialización
            if (context.User.Identity?.IsAuthenticated == true && 
                !context.Request.Path.Value?.Contains("/Auth/login") == true &&
                !context.Request.Path.Value?.Contains("/Auth/initialize") == true)
            {
                try
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                    {
                        var usuario = await dbContext.Usuarios.FindAsync(userId);
                        if (usuario != null && usuario.EstadoUsuario != "Activo")
                        {
                            _logger.LogWarning($"Usuario inactivo intentando acceder: ID {userId}, Email: {usuario.EmailUsuario}");
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                            {
                                message = "Usuario inactivo. Su sesión ha sido cerrada.",
                                error = "Usuario inactivo"
                            }));
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al validar estado del usuario en middleware");
                    // Continuar con la solicitud si hay error en la validación
                }
            }

            await _next(context);
        }
    }

    // Método de extensión para facilitar el uso
    public static class UserStatusValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserStatusValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserStatusValidationMiddleware>();
        }
    }
}

