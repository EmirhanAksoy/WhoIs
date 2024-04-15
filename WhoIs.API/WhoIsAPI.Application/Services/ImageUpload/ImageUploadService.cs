

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;
using WhoIsAPI.Domain.Extensions;
using WhoIsAPI.Domain.Models;
using WhoIsAPI.Persistence.Repositories.ImageUpload;

namespace WhoIsAPI.Application.Services.ImageUpload;

public class ImageUploadService : IImageUploadService
{
    private readonly ILogger<ImageUploadService> _logger;
    private readonly IImageUploadRepository _imageUploadRepository;
    public ImageUploadService(ILogger<ImageUploadService> logger, IImageUploadRepository imageUploadRepository)
    {
        _logger = logger;
        _imageUploadRepository = imageUploadRepository;
    }
    public async Task<Response<bool>> UploadImages(IFormFile zipFile, string imageFolderPath)
    {
        try
        {
            if (!Directory.Exists(imageFolderPath))
                Directory.CreateDirectory(imageFolderPath);

            List<string> imagePaths = [];
            using (var archive = ZipFile.OpenRead(zipFile.FileName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.FullName.IsImageFile())
                        {
                            var guidFileName = Guid.NewGuid().ToString() + Path.GetExtension(entry.FullName);
                            var imagePath = Path.Combine(imageFolderPath, guidFileName);
                            entry.ExtractToFile(imagePath);
                            imagePaths.Add(imagePath);
                        }
                    }
                }
            }
            List<ImageUniqueIdPair> images = imagePaths.ConvertAll(imagePath => new ImageUniqueIdPair(Guid.NewGuid().ToString(), imagePath));
            return await _imageUploadRepository.InsertImagePaths(images);  
        }
        catch (Exception ex)
        {
            IError error = new ImageBulkUploadError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} {fileName}", error.ErrorCode, error.ErrorMessage, zipFile.FileName);
            return Response<bool>.ErrorResult(error, ex);
        }
    }
}
