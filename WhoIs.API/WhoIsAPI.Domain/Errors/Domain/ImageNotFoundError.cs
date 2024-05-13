namespace WhoIsAPI.Domain.Errors;

public class ImageNotFoundError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageNotFoundError;

    string IError.ErrorCode => "ImageNotFoundError_S";

    string IError.ErrorMessage => "Image not found";
}
