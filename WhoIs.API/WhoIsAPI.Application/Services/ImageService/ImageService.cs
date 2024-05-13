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

    public async Task<Response<List<FaceInfo>>> GetFaces()
    {
        return Response<List<FaceInfo>>.SuccessResult(await _imageProcessRepository.GetFaces());
    }

    public async Task<Response<bool>> ProcessImages()
    {

        ImagePathPair? imagePathPair = await _imageProcessRepository.GetUnprocessedImage();

        if (imagePathPair is null)
        {
            _logger.LogInformation("No image record found in database");

            return Response<bool>.SuccessResult(true);
        }


        if (!File.Exists(imagePathPair.ImagePath))
        {
            bool imageDeleteResponse = await _imageProcessRepository.DeleteImage(imagePathPair.ImageId);
            return Response<bool>.HandleErrorResult<ImageNotFoundError>(_logger, "Image file is not exists {@image} {@response} ", imagePathPair, imageDeleteResponse);
        }

        byte[] imageBytes = File.ReadAllBytes(imagePathPair.ImagePath);

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
            return Response<bool>.HandleErrorResult<ImageProcessServiceError>(_logger, "Image process service failed {responseBody}", responseBody);
        }

        if (imageProcessResponse.Count == 0)
        {
            _logger.LogInformation("No face found in this image {@imageInfo}", imagePathPair);

            bool isImageWithNoFaceUpdated = await _imageProcessRepository.UpdateImageAsProcessed(imagePathPair.ImageId);

            if (!isImageWithNoFaceUpdated)
            {
                return Response<bool>.HandleErrorResult<ImageSetAsProcessedError>(_logger, "An error occured while setting image as processed.");
            }
        }

        List<ImageFaceMapping> imageFaceMappings = imageProcessResponse.Faces.ConvertAll(x => new ImageFaceMapping(x.Id, imagePathPair?.ImageId ?? string.Empty));

        bool isImageFaceMapInserted = await _imageProcessRepository.InsertImageFaceMappings(imageFaceMappings);

        if (!isImageFaceMapInserted)
            return Response<bool>.HandleErrorResult<ImageFaceMappingInsertError>(_logger, "Image face mapping insertion failed {@imageFaceMappings}", imageFaceMappings);

        bool isImageUpdated = await _imageProcessRepository.UpdateImageAsProcessed(imagePathPair.ImageId);

        if (!isImageUpdated)
            return Response<bool>.HandleErrorResult<ImageSetAsProcessedError>(_logger, "An error occured while setting image as processed.");

        return Response<bool>.SuccessResult(isImageUpdated);

    }

    public async Task<Response<bool>> UpdateFaceName(string imageId, string name)
    {

        bool isNameInUseResponse = await _imageProcessRepository.CheckIfFaceNameExists(imageId, name);
        if (isNameInUseResponse)
            Response<bool>.HandleErrorResult<FaceNameAlreadyInUseError>(_logger, "{Name} is already in use in another face image", name);

        bool isFaceNameUpdated = await _imageProcessRepository.UpdateFaceName(imageId, name);

        if (!isFaceNameUpdated)
            return Response<bool>.HandleErrorResult<UpdateFaceNameError>(_logger, "Face name update failed.");

        return Response<bool>.SuccessResult(isFaceNameUpdated);
    }

    public async Task<Response<List<string>>> GetImageIdsByFaceName(string faceNameSearchText)
    {
        return Response<List<string>>.SuccessResult(await _imageProcessRepository.GetImageIdsByFaceName(faceNameSearchText));
    }

    public async Task<Response<string>> GetImagePath(string imageId, bool isFaceImage)
    {
        return Response<string>.SuccessResult(await _imageProcessRepository.GetImagePath(imageId, isFaceImage));
    }

    public async Task<Response<bool>> UploadImages(IFormFile zipFile, string imageFolderPath)
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

        List<ImagePathPair> images = [];
        using (var archive = ZipFile.OpenRead(filePath))
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    if (entry.FullName.IsImageFile())
                    {

                        string imageId = Guid.NewGuid().ToString();
                        string guidFileName = imageId + Path.GetExtension(entry.FullName);
                        string imagePath = Path.Combine(imageFolderPath, guidFileName);
                        entry.ExtractToFile(imagePath);
                        images.Add(new(imageId, imagePath));
                    }
                }
            }
        }

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Response<bool>.SuccessResult(await _imageProcessRepository.InsertImagePaths(images));

    }
}
