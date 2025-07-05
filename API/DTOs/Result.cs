namespace API.DTOs;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;

    private Result(bool isSuccess, T? data, string message, string errorCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T data, string message = "")
    {
        return new Result<T>(true, data, message, string.Empty);
    }

    public static Result<T> Failure(string errorCode, string message)
    {
        return new Result<T>(false, default, message, errorCode);
    }

    public static Result<T> Failure(T data, string errorCode, string message)
    {
        return new Result<T>(false, data, message, errorCode);
    }
}

public class Result
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;

    private Result(bool isSuccess, string message, string errorCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public static Result Success(string message = "")
    {
        return new Result(true, message, string.Empty);
    }

    public static Result Failure(string errorCode, string message)
    {
        return new Result(false, message, errorCode);
    }
}
