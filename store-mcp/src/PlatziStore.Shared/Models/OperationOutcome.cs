namespace PlatziStore.Shared.Models;

public record OperationOutcome<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    private OperationOutcome() { }

    public static OperationOutcome<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static OperationOutcome<T> Failure(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}
