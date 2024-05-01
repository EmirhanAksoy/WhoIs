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
            // select 1 image which is not processed and not softly deleted

            Response<ImageUniqueIdPair> imageUniqueIdPairResponse = await _imageProcessRepository.GetUnprocessedImage();

            if (!imageUniqueIdPairResponse.Success)
            {
                return Response<bool>.MapError(imageUniqueIdPairResponse);
            }

            if (imageUniqueIdPairResponse.Data is null)
            {
                _logger.LogInformation("No image record found in database");

                return Response<bool>.SuccessResult(true);
            }


            if (!File.Exists(imageUniqueIdPairResponse.Data.ImagePath))
            {
                Response<bool> imageDeleteResponse = await _imageProcessRepository.DeleteImage(imageUniqueIdPairResponse.Data.UniqueId);

                IError error = new ImageNotFoundError();
                _logger.LogError(error.EventId, "Image file is not exists {Code} {Message} {@iamge} {@response} ", error.ErrorCode, error.ErrorMessage, imageUniqueIdPairResponse?.Data, imageDeleteResponse);
                return Response<bool>.ErrorResult(error);
            }

            byte[] imageBytes = File.ReadAllBytes(imageUniqueIdPairResponse.Data.ImagePath);

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
                IError error = new ImageProcessServiceError();
                _logger.LogError(error.EventId, "{Code} {Message} ", error.ErrorCode, error.ErrorMessage);
                return Response<bool>.ErrorResult(error);
            }

            if (imageProcessResponse.Count == 0)
            {
                _logger.LogInformation("No face found in this image {@imageInfo}", imageUniqueIdPairResponse);

                return await _imageProcessRepository.SetImageAsProcessed(imageUniqueIdPairResponse.Data.UniqueId);
            }

            List<ImageFaceMapping> imageFaceMappings = imageProcessResponse.Faces.ConvertAll(x => new ImageFaceMapping(x.Id, imageUniqueIdPairResponse?.Data?.UniqueId ?? string.Empty));

            Response<bool> imageFaceMapInsertResponse = await _imageProcessRepository.InsertImageFaceMappings(imageFaceMappings);

            if (!imageFaceMapInsertResponse.Success)
                return imageFaceMapInsertResponse;

            return await _imageProcessRepository.SetImageAsProcessed(imageUniqueIdPairResponse.Data.UniqueId);
        }
        catch (Exception ex)
        {
            IError error = new ImageProcessError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} ", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
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
                IError error = new ImageProcessServiceError();
                _logger.LogError(error.EventId, "{Code} {Message} ", error.ErrorCode, error.ErrorMessage);
                return Response<bool>.ErrorResult(error);
            }
            return await _imageProcessRepository.UpdateFaceName(imageId, name);
        }
        catch (Exception ex)
        {
            IError error = new ImageProcessError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} ", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
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
            List<ImageUniqueIdPair> images = imagePaths.ConvertAll(imagePath => new ImageUniqueIdPair(Guid.NewGuid().ToString(), imagePath));

            if (File.Exists(filePath))
                File.Delete(filePath);

            return await _imageProcessRepository.InsertImagePaths(images);
        }
        catch (Exception ex)
        {
            IError error = new ImageBulkUploadError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} {fileName}", error.ErrorCode, error.ErrorMessage, zipFile.FileName);
            return Response<bool>.ErrorResult(error, ex);
        }
    }
}
