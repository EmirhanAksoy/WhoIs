using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageRepository;

public interface IImageRepository
{
    Task<ImagePathPair?> GetUnprocessedImage();

    Task<List<FaceInfo>> GetFaces();

    Task<List<string>> GetImageIdsByFaceName(string faceNameSearchText);

    Task<string> GetImagePath(string imageId, bool isFaceImage);

    Task<bool> CheckIfFaceNameExists(string faceId, string name);

    Task<bool> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings);

    Task<bool> InsertImagePaths(List<ImagePathPair> images);

    Task<bool> UpdateImageAsProcessed(string imageId);

    Task<bool> UpdateFaceName(string faceId, string name);

    Task<bool> DeleteImage(string imageId);
}
