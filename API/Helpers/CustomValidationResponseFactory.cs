using Microsoft.AspNetCore.Mvc;

namespace API.Helpers
{
  public static class CustomValidationResponseFactory
  {
    public static IActionResult CreateValidationProblemResponse(ActionContext context)
    {
      var errors = context.ModelState
        .Where(x => x.Value!.Errors.Count > 0)
        .ToDictionary(
          kvp => kvp.Key,
          kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
        );
        
      var problem = new ValidationProblemDetails(errors)
      {
        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        Title = "Validación fallida",
        Status = 400,
        Instance = context.HttpContext?.Request?.Path,
        Detail = "Uno o más errores de validación ocurrieron."
      };
      return new BadRequestObjectResult(problem)
      {
        ContentTypes = { "application/problem+json" }
      };
    }
  }
}
