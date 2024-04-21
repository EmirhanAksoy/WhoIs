namespace WhoIsAPI.Domain.Errors;

public class ImageBulkInsertError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageBulkInsertError;

    string IError.ErrorCode => "ImageBulkInsertError";

    string IError.ErrorMessage => "An error occurred while insering image paths";
}
