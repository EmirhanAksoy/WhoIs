namespace WhoIsAPI.Domain.Errors;

public class ImageProcessServiceError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageProcessServiceError;

    string IError.ErrorCode => "ImageProcessServiceError";

    string IError.ErrorMessage => "An error occurred on image process service";
}
