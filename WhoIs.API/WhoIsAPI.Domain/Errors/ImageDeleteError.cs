namespace WhoIsAPI.Domain.Errors;

public class ImageDeleteError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageDeleteError;

    string IError.ErrorCode => "ImageDeleteError_R";

    string IError.ErrorMessage => "An error occured while deleting image";
}

