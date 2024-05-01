namespace WhoIsAPI.Domain.Errors;

public class UpdateFaceNameError : IError
{
    int IError.EventId => (int)ErrorCodes.UpdateFaceNameError;

    string IError.ErrorCode => "UpdateFaceNameError";

    string IError.ErrorMessage => "An error occurred while updating face name";
}
