using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Persistence.Repositories.ImageUpload;

public interface IImageUploadRepository
{
    Task<Response<bool>> InsertImagePaths(List<ImageUniqueIdPair> images);
}
