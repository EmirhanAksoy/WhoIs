namespace WhoIsAPI.Domain.Errors;

public class ImageBulkUploadError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageBulkUploadError;

    string IError.ErrorCode => "ImageBulkUploadError";

    string IError.ErrorMessage => "An error occurred while uploading images";
}
