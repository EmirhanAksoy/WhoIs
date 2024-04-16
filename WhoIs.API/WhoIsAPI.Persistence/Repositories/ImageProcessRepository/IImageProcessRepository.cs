using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageProcessRepository;

public interface IImageProcessRepository
{
    Task<Response<ImageUniqueIdPair>> GetUnprocessedImage();

    Task<Response<bool>> DeleteImage(string imageId);

    Task<Response<bool>> InsertImageFaceMappings(List<ImageFaceMapping> imageFaceMappings);
}
