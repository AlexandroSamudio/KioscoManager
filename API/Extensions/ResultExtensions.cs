using API.Constants;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return new NoContentResult();
            }

            return result.ErrorCode switch
            {
                ErrorCodes.EntityNotFound => new NotFoundObjectResult(result.Message),
                ErrorCodes.FieldExists => new ConflictObjectResult(result.Message),
                ErrorCodes.EmptyField => new BadRequestObjectResult(result.Message),
                ErrorCodes.InvalidOperation => new BadRequestObjectResult(result.Message),
                ErrorCodes.ValidationError => new BadRequestObjectResult(result.Message),
                ErrorCodes.Forbidden => new ObjectResult(result.Message) { StatusCode = 403 },
                ErrorCodes.InvalidCurrentPassword => new BadRequestObjectResult("Contraseña actual incorrecta"),
                ErrorCodes.Unauthorized => new UnauthorizedObjectResult("No autorizado"),
                _ => new ObjectResult("Error interno del servidor") { StatusCode = 500 }
            };
        }

        public static ActionResult<T> ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Data ?? throw new InvalidOperationException("Los resultados exitosos deben contener datos válidos");
            }

            return result.ErrorCode switch
            {
                ErrorCodes.EntityNotFound => new NotFoundObjectResult(result.Message),
                ErrorCodes.FieldExists => new ConflictObjectResult(result.Message),
                ErrorCodes.EmptyField => new BadRequestObjectResult(result.Message),
                ErrorCodes.InvalidOperation => new BadRequestObjectResult(result.Message),
                ErrorCodes.ValidationError => new BadRequestObjectResult(result.Message),
                ErrorCodes.Forbidden => new ObjectResult(result.Message) { StatusCode = 403 },
                ErrorCodes.InvalidCurrentPassword => new BadRequestObjectResult("Contraseña actual incorrecta"),
                ErrorCodes.Unauthorized => new UnauthorizedObjectResult("No autorizado"),
                _ => new ObjectResult("Error interno del servidor") { StatusCode = 500 }
            };
        }

        public static ActionResult<T> ToActionResult<T>(this Result<T> result, Func<T, ActionResult> successAction)
        {
            if (result.IsSuccess)
            {
                return result.Data != null
                ? successAction(result.Data)
                : throw new InvalidOperationException("Los resultados exitosos deben contener datos válidos");
            }

            return result.ToActionResult();
        }
    }
}
