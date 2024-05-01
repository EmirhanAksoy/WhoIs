namespace WhoIsAPI.Domain.Errors;

public class CheckIfFaceNameExistsError : IError
{
    int IError.EventId => (int)ErrorCodes.CheckIfFaceNameExistsError;

    string IError.ErrorCode => "CheckIfFaceNameExistsError_R";

    string IError.ErrorMessage => "An error occurred while checking face name exist";
}
