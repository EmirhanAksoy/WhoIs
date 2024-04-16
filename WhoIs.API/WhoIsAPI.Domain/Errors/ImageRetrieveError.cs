namespace WhoIsAPI.Domain.Errors;

public class ImageRetrieveError : IError
{
    int IError.EventId => 10000;

    string IError.ErrorCode => "ImageFetchError";

    string IError.ErrorMessage => "An error occurred while retrieving image";
}
