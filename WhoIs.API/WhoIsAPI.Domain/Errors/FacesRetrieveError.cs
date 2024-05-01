namespace WhoIsAPI.Domain.Errors;

public class FacesRetrieveError : IError
{
    int IError.EventId => (int)ErrorCodes.FacesRetrieveError;

    string IError.ErrorCode => "FacesRetrieveError_R";

    string IError.ErrorMessage => "An error occurred while retrieving faces";
}
