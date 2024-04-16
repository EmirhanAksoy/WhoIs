

namespace WhoIsAPI.Domain.Errors;

public class ImageProcessError : IError
{
    int IError.EventId => 7000;

    string IError.ErrorCode => "ImageProcessError";

    string IError.ErrorMessage => "An error occurred while processing image";
}
