

namespace WhoIsAPI.Domain.Errors;
public class GetImageIdsByFaceNameError : IError
{
    int IError.EventId => (int)ErrorCodes.GetImageIdsByFaceNameError;

    string IError.ErrorCode => "GetImageIdsByFaceNameError";

    string IError.ErrorMessage => "An error occurred while retrieving image ids with face name";
}
