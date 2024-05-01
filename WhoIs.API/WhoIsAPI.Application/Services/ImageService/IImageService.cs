using Microsoft.AspNetCore.Http;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Application.Services.ImageService;

public interface IImageService
{
    Task<Response<bool>> ProcessImages();
    Task<Response<List<FaceInfo>>> GetFaces();
    Task<Response<string>> GetFaceImagePath(string imageId);
    Task<Response<bool>> UpdateFaceName(string imageId,string name);
    Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText);
    Task<Response<bool>> UploadImages(IFormFile zipFile, string imageFolderPath);
}
