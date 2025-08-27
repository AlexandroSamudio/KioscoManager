using API.Constants;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ResultExtensions
    {
        private class ErrorMetadata
        {
            public required int StatusCode { get; init; }
            public required string Title { get; init; }
            public required string TypeUrl { get; init; }
            public required string Key { get; init; }
            public required Func<object, ObjectResult> ResultFactory { get; init; }
        }

        private static readonly Dictionary<string, ErrorMetadata> ErrorMetadataMap = new()
        {
            [ErrorCodes.EntityNotFound] = new()
            {
                StatusCode = 404,
                Title = "Recurso no encontrado",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Key = "resource",
                ResultFactory = responseObj => WithProblemContentType(new NotFoundObjectResult(responseObj))
            },
            [ErrorCodes.FieldExists] = new()
            {
                StatusCode = 409,
                Title = "Conflicto de recursos",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Key = "field",
                ResultFactory = responseObj => WithProblemContentType(new ConflictObjectResult(responseObj))
            },
            [ErrorCodes.InsufficientStock] = new()
            {
                StatusCode = 409,
                Title = "Stock insuficiente",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Key = "stock",
                ResultFactory = responseObj => WithProblemContentType(new ConflictObjectResult(responseObj))
            },
            [ErrorCodes.EmptyField] = new()
            {
                StatusCode = 400,
                Title = "Campo requerido vacío",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Key = "field",
                ResultFactory = responseObj => WithProblemContentType(new BadRequestObjectResult(responseObj))
            },
            [ErrorCodes.InvalidOperation] = new()
            {
                StatusCode = 400,
                Title = "Operación inválida",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Key = "operation",
                ResultFactory = responseObj => WithProblemContentType(new BadRequestObjectResult(responseObj))
            },
            [ErrorCodes.ValidationError] = new()
            {
                StatusCode = 400,
                Title = "Error de validación",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Key = "validation",
                ResultFactory = responseObj => WithProblemContentType(new BadRequestObjectResult(responseObj))
            },
            [ErrorCodes.Forbidden] = new()
            {
                StatusCode = 403,
                Title = "Acceso prohibido",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                Key = "authorization",
                ResultFactory = responseObj => WithProblemContentType(new ObjectResult(responseObj) { StatusCode = 403 })
            },
            [ErrorCodes.InvalidCurrentPassword] = new()
            {
                StatusCode = 400,
                Title = "Contraseña incorrecta",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Key = "password",
                ResultFactory = responseObj => WithProblemContentType(new BadRequestObjectResult(responseObj))
            },
            [ErrorCodes.Unauthorized] = new()
            {
                StatusCode = 401,
                Title = "Acceso no autorizado",
                TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                Key = "authentication",
                ResultFactory = responseObj => WithProblemContentType(new UnauthorizedObjectResult(responseObj))
            }
        };

        private static readonly ErrorMetadata DefaultErrorMetadata = new()
        {
            StatusCode = 500,
            Title = "Error interno del servidor",
            TypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Key = "unknown",
            ResultFactory = responseObj => WithProblemContentType(new ObjectResult(responseObj) { StatusCode = 500 })
        };

        private static ErrorMetadata GetErrorMetadata(string errorCode)
        {
            return ErrorMetadataMap.TryGetValue(errorCode, out var metadata) ? metadata : DefaultErrorMetadata;
        }
        private static ValidationProblemDetails CreateErrorResponseObject(string errorCode, string? message)
        {
            var metadata = GetErrorMetadata(errorCode);
            var detail = "Uno o más errores han ocurrido.";
            var errorsDict = new Dictionary<string, string[]>
            {
                { metadata.Key, new[] { message ?? "Error desconocido" } }
            };

            var problemDetails = new ValidationProblemDetails(errorsDict)
            {
                Type = metadata.TypeUrl,
                Title = metadata.Title,
                Status = metadata.StatusCode,
                Detail = detail
            };
            return problemDetails;
        }

        private static ObjectResult CreateObjectResultForErrorCode(string errorCode, object responseObj)
        {
            var metadata = GetErrorMetadata(errorCode);
            return metadata.ResultFactory(responseObj);
        }

        private static T WithProblemContentType<T>(T result) where T : ObjectResult
        {
            result.ContentTypes.Clear();
            result.ContentTypes.Add("application/problem+json");
            return result;
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
    }
}
