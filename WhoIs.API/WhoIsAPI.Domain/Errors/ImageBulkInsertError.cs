namespace WhoIsAPI.Domain.Errors;

public class ImageBulkInsertError : IError
{
    int IError.EventId => 5000;

    string IError.ErrorCode => "ImageBulkInsertError";

    string IError.ErrorMessage => "An error occurred while insering image paths";
}
