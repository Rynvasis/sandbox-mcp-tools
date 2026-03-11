namespace PlatziStore.Shared.Exceptions;

public sealed class ExternalServiceException : Exception
{
    public int? StatusCode { get; }
    public string ServiceName { get; }
    public string ErrorDetail { get; }

    public ExternalServiceException(string serviceName, string errorDetail, int? statusCode = null, Exception? innerException = null)
        : base($"Error from external service '{serviceName}': {errorDetail}{(statusCode.HasValue ? $" (Status: {statusCode})" : string.Empty)}", innerException)
    {
        ServiceName = serviceName;
        ErrorDetail = errorDetail;
        StatusCode = statusCode;
    }
}
