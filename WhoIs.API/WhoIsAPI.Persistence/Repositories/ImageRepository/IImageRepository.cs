using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageRepository;

public interface IImageRepository
{
    Task<Response<ImageUniqueIdPair>> GetUnprocessedImage();

    Task<Response<List<FaceInfo>>> GetFaces();

    Task<Response<string>> GetFaceImagePath(string imageId);

    Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText);

    Task<Response<bool>> CheckIfFaceNameExists(string imageId, string name);

    Task<Response<bool>> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings);

    Task<Response<bool>> InsertImagePaths(List<ImageUniqueIdPair> images);

    Task<Response<bool>> UpdateImageAsProcessed(string imageId);

    Task<Response<bool>> UpdateFaceName(string imageId, string name);

    Task<Response<bool>> DeleteImage(string imageId);

}
