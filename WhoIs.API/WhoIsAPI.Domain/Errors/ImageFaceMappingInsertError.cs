namespace WhoIsAPI.Domain.Errors;

public class ImageFaceMappingInsertError : IError
{
    int IError.EventId => 1200;

    string IError.ErrorCode => "ImageFaceMappingInsertError";

    string IError.ErrorMessage => "An error occured while insering image-face mappings";
}
