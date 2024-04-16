namespace WhoIsAPI.Domain.Errors;

public class ImageDeleteError : IError
{
    int IError.EventId => 1100;

    string IError.ErrorCode => "ImageDeleteError";

    string IError.ErrorMessage => "An error occured while deleting image";
}

