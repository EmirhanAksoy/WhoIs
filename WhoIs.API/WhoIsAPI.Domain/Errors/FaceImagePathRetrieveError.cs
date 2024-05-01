

namespace WhoIsAPI.Domain.Errors;

public class FaceImagePathRetrieveError : IError
{
    int IError.EventId => (int)ErrorCodes.FaceImagePathRetrieveError;

    string IError.ErrorCode => "FaceImagePathRetrieveError_R";

    string IError.ErrorMessage => "An error occurred while retrieving face image path";
}
