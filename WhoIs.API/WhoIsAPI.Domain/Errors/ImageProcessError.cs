

namespace WhoIsAPI.Domain.Errors;

public class ImageProcessError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageProcessError;

    string IError.ErrorCode => "ImageProcessError";

    string IError.ErrorMessage => "An error occurred while processing image";
}
