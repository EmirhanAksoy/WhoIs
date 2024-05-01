namespace WhoIsAPI.Domain.Errors;

public class ImageFaceMappingInsertError : IError
{
    int IError.EventId => (int)ErrorCodes.ImageFaceMappingInsertError;

    string IError.ErrorCode => "ImageFaceMappingInsertError_R";

    string IError.ErrorMessage => "An error occured while insering image-face mappings";
}
