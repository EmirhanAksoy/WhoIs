using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Application.Services.ImageProcess;

public interface IImageProcessService
{
    Task<Response<bool>> ProcessImages();
    Task<Response<List<FaceInfo>>> GetFaces();
    Task<Response<string>> GetFaceImagePath(string imageId);
}
