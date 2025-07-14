namespace API.Entities;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string? ErrorCode { get; init; }

    private Result(bool isSuccess, T? data, string? message, string? errorCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        ErrorCode = errorCode;
    }

    //Crea un resultado exitoso incluyendo datos
    public static Result<T> Success(T? data, string? message = null)
    {
        return new Result<T>(true, data, message,null);
    }

    // Crea un resultado con fallo con datos parciales.
    // Útil cuando se necesita retornar información incluso en caso de error.
    public static Result<T> Failure(string errorCode, string? message = null)
    {
        return new Result<T>(false, default!, message, errorCode);
    }
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public string ErrorCode { get; init; } = string.Empty;

    private Result(bool isSuccess, string? message, string errorCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public static Result Success(string? message = null)
    {
        return new Result(true, message, string.Empty);
    }

    public static Result Failure(string errorCode, string? message = null)
    {
        return new Result(false, message, errorCode);
    }
}
