namespace WhoIsAPI.Domain.Errors;

public class ImageSetAsProcessedError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageSetAsProcessedError;

    string IError.ErrorCode => "ImageSetAsProcessedError_R";

    string IError.ErrorMessage => "An error occurred while setting image as processed";
}