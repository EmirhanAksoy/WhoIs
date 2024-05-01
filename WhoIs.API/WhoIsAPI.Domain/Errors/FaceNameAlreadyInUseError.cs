namespace WhoIsAPI.Domain.Errors;

public class FaceNameAlreadyInUseError : IError
{
    int IError.EventId => (int)ErrorCodes.FaceNameAlreadyInUseError;

    string IError.ErrorCode => "FaceNameAlreadyInUseError_S";

    string IError.ErrorMessage => "Face name is already in use";
}
