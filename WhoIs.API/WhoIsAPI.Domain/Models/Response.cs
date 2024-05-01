using Microsoft.Extensions.Logging;
using WhoIsAPI.Domain.Errors;

namespace WhoIsAPI.Domain;

public class Response<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
    public string ExceptionMessage { get; set; } = string.Empty;

    public Response() => Success = true;

    private Response(T data) : this()
    {
        Data = data;
    }

    private Response(string errorMessage, string errorCode) : this()
    {
        Success = false;
        Errors.Add(errorMessage);
        ErrorCode = errorCode;
    }

    private Response(List<string> errors, string errorCode) : this()
    {
        Success = false;
        Errors.AddRange(errors);
        ErrorCode = errorCode;
    }

    private Response(string errorMessage, string errorCode, Exception exception) : this()
    {
        Success = false;
        Errors.Add(errorMessage);
        ExceptionMessage = exception.Message;
        ErrorCode = errorCode;
    }

    public static Response<T> SuccessResult(T data)
    {
        return new Response<T>(data);
    }
    
    public static Response<T> HandleErrorResult<TError>(ILogger logger, string logMessage, params object[] logParams) where TError : class, IError, new()
    {
        TError error = new();
        string errorLog = "{ErrorCode} {ErrorMessage}";
        logger.LogError(error.EventId, $"{errorLog} {logMessage}", error.ErrorCode, error.ErrorMessage, logParams);
        return new Response<T>(error.ErrorMessage, error.ErrorCode);
    }

    public static Response<T> HandleException<TError>(Exception exception, ILogger logger) where TError : class, IError, new()
    {
        TError error = new();
        logger.LogError(error.EventId, exception, "{Code} {Message}", error.ErrorCode, error.ErrorMessage);
        return new Response<T>(error.ErrorMessage, error.ErrorCode, exception);
    }

    public static Response<T> MapError<TResponse>(Response<TResponse> response)
    {
        return new Response<T>(response.Errors, response.ErrorCode);
    }
}
