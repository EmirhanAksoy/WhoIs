namespace WhoIsAPI.Domain.Errors;

public enum ErrorCodes
{
    ImageBulkInsertError = 1000,
    ImageBulkUploadError,
    ImageDeleteError,
    ImageFaceMappingInsertError,
    ImageNotFoundError,
    ImageProcessError,
    ImageProcessServiceError,
    ImageRetrieveError,
    ImageSetAsProcessedError,
    FacesRetrieveError,
    FaceImagePathRetrieveError,
    GetImageIdsByFaceNameError,
    CheckIfFaceNameExistsError,
    UpdateFaceNameError,
    FaceNameAlreadyInUseError
}
