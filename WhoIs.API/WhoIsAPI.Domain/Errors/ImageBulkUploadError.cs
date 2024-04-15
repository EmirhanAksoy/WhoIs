namespace WhoIsAPI.Domain.Errors;

public class ImageBulkUploadError : IError
{
    int IError.EventId => 6000;

    string IError.ErrorCode => "ImageBulkUploadError";

    string IError.ErrorMessage => "An error occurred while uploading images";
}
