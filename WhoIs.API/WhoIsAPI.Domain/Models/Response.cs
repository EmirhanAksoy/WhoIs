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

    public static Response<T> ErrorResult(string errorCode, string errorMessage)
    {
        return new Response<T>(errorMessage, errorCode);
    }

    public static Response<T> ErrorResult(IError error)
    {
        return new Response<T>(error.ErrorMessage, error.ErrorCode);
    }

    public static Response<T> ErrorResult(IError error, Exception exception)
    {
        return new Response<T>(error.ErrorMessage, error.ErrorCode, exception);
    }

    public static Response<T> ErrorResult(string errorCode, List<string> errorMessages)
    {
        return new Response<T>(errorMessages, errorCode);
    }
}
