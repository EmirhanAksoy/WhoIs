namespace WhoIsAPI.Domain.Errors;

public class ImageProcessServiceError : IError
{
    int IError.EventId => 9000;

    string IError.ErrorCode => "ImageProcessServiceError";

    string IError.ErrorMessage => "An error occurred on image process service";
}
