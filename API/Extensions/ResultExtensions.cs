using API.Constants;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ResultExtensions
    {
        private static object CreateErrorResponseObject(string errorCode, string? message)
        {
            return new
            {
                type = GetErrorTypeUrl(errorCode),
                title = GetErrorTitle(errorCode),
                status = GetStatusCode(errorCode),
                detail = message ?? "Uno o más errores de validación ocurrieron.",
                errors = new Dictionary<string, string[]>
                {
                    { GetErrorKey(errorCode), new[] { message ?? "Error desconocido" } }
                }
            };
        }

        private static ObjectResult CreateObjectResultForErrorCode(string errorCode, object responseObj)
        {
            return errorCode switch
            {
                ErrorCodes.EntityNotFound => new NotFoundObjectResult(responseObj),
                ErrorCodes.FieldExists => new ConflictObjectResult(responseObj),
                ErrorCodes.EmptyField => new BadRequestObjectResult(responseObj),
                ErrorCodes.InvalidOperation => new BadRequestObjectResult(responseObj),
                ErrorCodes.ValidationError => new BadRequestObjectResult(responseObj),
                ErrorCodes.Forbidden => new ObjectResult(responseObj) { StatusCode = 403 },
                ErrorCodes.InvalidCurrentPassword => new BadRequestObjectResult(responseObj),
                ErrorCodes.Unauthorized => new UnauthorizedObjectResult(responseObj),
                "UnknownError" => new ObjectResult(responseObj) { StatusCode = 500 },
                _ => new ObjectResult(responseObj) { StatusCode = 500 }
            };
        }
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
            {
                return new NoContentResult();
            }

            return CreateErrorResponse(result);
        }

        public static ActionResult<T> ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Data ?? throw new InvalidOperationException("Los resultados exitosos deben contener datos válidos");
            }

            return CreateErrorResponseGeneric<T>(result);
        }

        public static ActionResult<T> ToActionResult<T>(this Result<T> result, Func<T, ActionResult> successAction)
        {
            if (result.IsSuccess)
            {
                return result.Data != null
                ? successAction(result.Data)
                : throw new InvalidOperationException("Los resultados exitosos deben contener datos válidos");
            }

            return CreateErrorResponseGeneric<T>(result);
        }

    private static ObjectResult CreateErrorResponse(Result result)
        {
            var responseObj = CreateErrorResponseObject(result.ErrorCode, result.Message);
            return CreateObjectResultForErrorCode(result.ErrorCode, responseObj);
        }

        private static ActionResult<T> CreateErrorResponseGeneric<T>(Result<T> result)
        {
            var errorCode = result.ErrorCode ?? "UnknownError";
            var responseObj = CreateErrorResponseObject(errorCode, result.Message);
            return CreateObjectResultForErrorCode(errorCode, responseObj);
        }

        private static string GetErrorTypeUrl(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.EntityNotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                ErrorCodes.FieldExists => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                ErrorCodes.EmptyField => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                ErrorCodes.InvalidOperation => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                ErrorCodes.ValidationError => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                ErrorCodes.Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                ErrorCodes.InvalidCurrentPassword => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                ErrorCodes.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                "UnknownError" => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            };
        }

        private static string GetErrorTitle(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.EntityNotFound => "Recurso no encontrado",
                ErrorCodes.FieldExists => "Conflicto de recursos",
                ErrorCodes.EmptyField => "Campo requerido vacío",
                ErrorCodes.InvalidOperation => "Operación inválida",
                ErrorCodes.ValidationError => "Error de validación",
                ErrorCodes.Forbidden => "Acceso prohibido",
                ErrorCodes.InvalidCurrentPassword => "Contraseña incorrecta",
                ErrorCodes.Unauthorized => "Acceso no autorizado",
                "UnknownError" => "Error desconocido",
                _ => "Error interno del servidor"
            };
        }

        private static int GetStatusCode(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.EntityNotFound => 404,
                ErrorCodes.FieldExists => 409,
                ErrorCodes.EmptyField => 400,
                ErrorCodes.InvalidOperation => 400,
                ErrorCodes.ValidationError => 400,
                ErrorCodes.Forbidden => 403,
                ErrorCodes.InvalidCurrentPassword => 400,
                ErrorCodes.Unauthorized => 401,
                "UnknownError" => 500,
                _ => 500
            };
        }

        private static string GetErrorKey(string errorCode)
        {
            return errorCode switch
            {
                ErrorCodes.EntityNotFound => "resource",
                ErrorCodes.FieldExists => "field",
                ErrorCodes.EmptyField => "field",
                ErrorCodes.InvalidOperation => "operation",
                ErrorCodes.ValidationError => "validation",
                ErrorCodes.Forbidden => "authorization",
                ErrorCodes.InvalidCurrentPassword => "password",
                ErrorCodes.Unauthorized => "authentication",
                "UnknownError" => "unknown",
                _ => "server"
            };
        }
    }
}
