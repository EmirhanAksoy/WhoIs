using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;
using WhoIsAPI.Domain.Extensions;
using WhoIsAPI.Domain.Models;
using WhoIsAPI.Persistence.Repositories.ImageRepository;

namespace WhoIsAPI.Application.Services.ImageService;

public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private readonly IImageRepository _imageProcessRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    public ImageService(ILogger<ImageService> logger,
        IImageRepository imageProcessRepository,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _imageProcessRepository = imageProcessRepository;
        _httpClientFactory = httpClientFactory;
    }

    public Task<Response<List<FaceInfo>>> GetFaces()
    {
        return _imageProcessRepository.GetFaces();
    }

    public Task<Response<string>> GetFaceImagePath(string imageId)
    {
        return _imageProcessRepository.GetFaceImagePath(imageId);
    }

    public async Task<Response<bool>> ProcessImages()
    {
        try
        {

            Response<ImagePathPair> imagePathPairResponse = await _imageProcessRepository.GetUnprocessedImage();

            if (!imagePathPairResponse.Success)
            {
                return Response<bool>.MapError(imagePathPairResponse);
            }

            if (imagePathPairResponse.Data is null)
            {
                _logger.LogInformation("No image record found in database");

                return Response<bool>.SuccessResult(true);
            }


            if (!File.Exists(imagePathPairResponse.Data.ImagePath))
            {
                Response<bool> imageDeleteResponse = await _imageProcessRepository.DeleteImage(imagePathPairResponse.Data.ImageId);
                return Response<bool>.HandleErrorResult<ImageNotFoundError>(_logger, "Image file is not exists {@image} {@response} ", imagePathPairResponse.Data, imageDeleteResponse);
            }

            byte[] imageBytes = File.ReadAllBytes(imagePathPairResponse.Data.ImagePath);

            // send image to the external image process service

            HttpClient httpClient = _httpClientFactory.CreateClient("facerec_service");
            MultipartFormDataContent formData = [];
            ByteArrayContent fileContent = new(imageBytes);
            fileContent.Headers.Add("Content-Type", "image/png");
            formData.Add(fileContent, "file", $"{Guid.NewGuid()}.png");
            HttpResponseMessage response = await httpClient.PostAsync("/detect_faces", formData);
            string responseBody = await response.Content.ReadAsStringAsync();
            ImageProcessResponse? imageProcessResponse = JsonSerializer.Deserialize<ImageProcessResponse>(responseBody, _jsonSerializerOptions);
            _logger.LogInformation("Image process service response {@response}", imageProcessResponse);
            if (imageProcessResponse is null)
            {
                return Response<bool>.HandleErrorResult<ImageProcessServiceError>(_logger, "Image process service failed {responseBody}",responseBody);
            }

            if (imageProcessResponse.Count == 0)
            {
                _logger.LogInformation("No face found in this image {@imageInfo}", imagePathPairResponse);

                return await _imageProcessRepository.UpdateImageAsProcessed(imagePathPairResponse.Data.ImageId);
            }

            List<ImageFaceMapping> imageFaceMappings = imageProcessResponse.Faces.ConvertAll(x => new ImageFaceMapping(x.Id, imagePathPairResponse?.Data?.ImageId ?? string.Empty));

            Response<bool> imageFaceMapInsertResponse = await _imageProcessRepository.InsertImageFaceMappings(imageFaceMappings);

            if (!imageFaceMapInsertResponse.Success)
                return imageFaceMapInsertResponse;

            return await _imageProcessRepository.UpdateImageAsProcessed(imagePathPairResponse.Data.ImageId);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageProcessError>(ex, _logger);
        }
    }

    public async Task<Response<bool>> UpdateFaceName(string imageId, string name)
    {
        try
        {
            Response<bool> isNameInUseResponse = await _imageProcessRepository.CheckIfFaceNameExists(imageId, name);
            if (!isNameInUseResponse.Success)
                return isNameInUseResponse;

            if (isNameInUseResponse.Data)
            {
                return Response<bool>.HandleErrorResult<FaceNameAlreadyInUseError>(_logger,"{Name} is already in use in another face image",name);
            }
            return await _imageProcessRepository.UpdateFaceName(imageId, name);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageProcessError>(ex, _logger);
        }
    }

    public Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText)
    {
        return _imageProcessRepository.GetImageIdsByFaceName(faceNameSearchText);
    }

    public async Task<Response<bool>> UploadImages(IFormFile zipFile, string imageFolderPath)
    {
        try
        {
            if (!Directory.Exists(imageFolderPath))
                Directory.CreateDirectory(imageFolderPath);


            var fileName = Guid.NewGuid().ToString() + ".zip";

            // Define the path where you want to save the zip file
            var filePath = Path.Combine(imageFolderPath, fileName);

            // Create a new FileStream to save the uploaded file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                // Copy the contents of the uploaded file to the FileStream asynchronously
                await zipFile.CopyToAsync(fileStream);
            }

            List<string> imagePaths = [];
            using (var archive = ZipFile.OpenRead(filePath))
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
            List<ImagePathPair> images = imagePaths.ConvertAll(imagePath => new ImagePathPair(Guid.NewGuid().ToString(), imagePath));

            if (File.Exists(filePath))
                File.Delete(filePath);

            return await _imageProcessRepository.InsertImagePaths(images);
        }
        catch (Exception ex)
        {
            return Response<bool>.HandleException<ImageBulkUploadError>(ex, _logger);
        }
    }
}
