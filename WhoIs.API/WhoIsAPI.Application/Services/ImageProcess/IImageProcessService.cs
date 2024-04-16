using WhoIsAPI.Domain;

namespace WhoIsAPI.Application.Services.ImageProcess;

public interface IImageProcessService
{
    Task<Response<bool>> ProcessImages();
}
