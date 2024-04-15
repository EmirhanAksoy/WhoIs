

using Microsoft.AspNetCore.Http;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Application.Services.ImageUpload;

public interface IImageUploadService
{
    Task<Response<bool>> UploadImages(IFormFile zipFile, string imageFolderPath);
}
