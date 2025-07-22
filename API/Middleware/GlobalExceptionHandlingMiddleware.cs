using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
                {
                    await HandleUnauthorizedResponseAsync(context);
                }
                else if (context.Response.StatusCode == 403 && !context.Response.HasStarted)
                {
                    await HandleForbiddenResponseAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Una excepcion no controlada ha ocurrido: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ArgumentOutOfRangeException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Argumento fuera de rango: {exception.Message}";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ArgumentException or InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Solicitud incorrecta: {exception.Message}";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Recurso no encontrado";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Ha ocurrido un error interno del servidor";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static async Task HandleUnauthorizedResponseAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                StatusCode = 401,
                Message = "Acceso no autorizado.",
                Details = "Debe proporcionar un token de autenticación válido para acceder a este recurso."
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static async Task HandleForbiddenResponseAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                StatusCode = 403,
                Message = "Acceso denegado."
            };

            var customMessage = context.Items["AuthorizationErrorMessage"] as string;
            if (!string.IsNullOrEmpty(customMessage))
            {
                response.Details = customMessage;
            }
            else
            {
                response.Details = "No tiene permisos suficientes para realizar esta acción.";
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
