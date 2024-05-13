namespace WhoIsAPI.Domain.Errors;

public enum ErrorCodes
{
    ImageBulkInsertError = 1000,
    UpdateFaceNameError,
    ImageSetAsProcessedError,
    ImageProcessServiceError,
    ImageNotFoundError,
    ImageFaceMappingInsertError,
    FaceNameAlreadyInUseError
}
