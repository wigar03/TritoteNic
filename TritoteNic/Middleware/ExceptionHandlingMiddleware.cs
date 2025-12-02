using System.Net;
using System.Text.Json;

namespace TritoteNic.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new
            {
                error = new
                {
                    message = "Ocurrió un error al procesar la solicitud.",
                    details = exception.Message
                }
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        error = new
                        {
                            message = "Error de validación.",
                            details = argEx.Message
                        }
                    };
                    break;

                case InvalidOperationException invOpEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        error = new
                        {
                            message = "Operación no válida.",
                            details = invOpEx.Message
                        }
                    };
                    break;

                case KeyNotFoundException keyEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = new
                    {
                        error = new
                        {
                            message = "Recurso no encontrado.",
                            details = keyEx.Message
                        }
                    };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new
                    {
                        error = new
                        {
                            message = "Error interno del servidor.",
                            details = exception.Message
                        }
                    };
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }

    // Método de extensión para facilitar el uso
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
