using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware
{
  public class GlobalExceptionHandlingMiddleware(RequestDelegate next,ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
    {
      try
      {
        await next(context);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Una excepcion inesperada ha ocurrido.");
        if (context.Response.HasStarted)
        {
          return;
        }
        var isDevelopment = context.RequestServices
                                    .GetService<IWebHostEnvironment>()?
                                    .IsDevelopment() ?? false;
        
        var problemDetails = new ProblemDetails
        {
          Type     = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
          Title    = "An unexpected error occurred.",
          Status   = (int)HttpStatusCode.InternalServerError,
          Detail   = isDevelopment
                       ? ex.Message
                       : "Un error ha ocurrido procesando su solicitud.",
          Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
      }
    }
  }
}