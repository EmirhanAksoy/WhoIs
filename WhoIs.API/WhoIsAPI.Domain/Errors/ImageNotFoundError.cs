namespace WhoIsAPI.Domain.Errors;

public class ImageNotFoundError : IError
{
    int IError.EventId => 8000;

    string IError.ErrorCode => "ImageNotFoundError";

    string IError.ErrorMessage => "Image not found";
}
