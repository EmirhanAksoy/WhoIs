namespace WhoIsAPI.Domain.Errors;

public class ImageRetrieveError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageRetrieveError;

    string IError.ErrorCode => "ImageFetchError_R";

    string IError.ErrorMessage => "An error occurred while retrieving image";
}
