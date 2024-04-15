namespace WhoIsAPI.Domain.Errors;

public interface IError
{
    public int EventId { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; } 
}
