using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageRepository;

public interface IImageRepository
{
    Task<Response<ImagePathPair>> GetUnprocessedImage();

    Task<Response<List<FaceInfo>>> GetFaces();

    Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText);

    Task<Response<string>> GetImagePath(string imageId, bool isFaceImage);

    Task<Response<bool>> CheckIfFaceNameExists(string faceId, string name);

    Task<Response<bool>> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings);

    Task<Response<bool>> InsertImagePaths(List<ImagePathPair> images);

    Task<Response<bool>> UpdateImageAsProcessed(string imageId);

    Task<Response<bool>> UpdateFaceName(string faceId, string name);

    Task<Response<bool>> DeleteImage(string imageId);
}
