namespace Microfinance.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static Result Success(string message = "Operation completed successfully.")
        => new() { Succeeded = true, Message = message };

    public static Result Failure(params string[] errors)
        => new() { Succeeded = false, Errors = errors };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T data, string message = "Success")
        => new() { Succeeded = true, Data = data, Message = message };

    public new static Result<T> Failure(params string[] errors)
        => new() { Succeeded = false, Errors = errors };
}