using Microsoft.Extensions.Logging;
using System.Text.Json;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Errors;
using WhoIsAPI.Domain.Models;
using WhoIsAPI.Persistence.Repositories.ImageProcessRepository;

namespace WhoIsAPI.Application.Services.ImageProcess;

public class ImageProcessService : IImageProcessService
{
    private readonly ILogger<ImageProcessService> _logger;
    private readonly IImageProcessRepository _imageProcessRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
    public ImageProcessService(ILogger<ImageProcessService> logger,
        IImageProcessRepository imageProcessRepository,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _imageProcessRepository = imageProcessRepository;
        _httpClientFactory = httpClientFactory;
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

            // read image stream via path

            if (!File.Exists(imageUniqueIdPairResponse?.Data?.ImagePath))
            {
                Response<bool> imageDeleteResponse = await _imageProcessRepository.DeleteImage(imageUniqueIdPairResponse?.Data?.UniqueId ?? string.Empty);

                IError error = new ImageNotFoundError();
                _logger.LogError(error.EventId, "Image file is not exists {Code} {Message} {@iamge} {@response} ", error.ErrorCode, error.ErrorMessage, imageUniqueIdPairResponse?.Data, imageDeleteResponse);
                return Response<bool>.ErrorResult(error);
            }

            byte[] imageBytes = File.ReadAllBytes(imageUniqueIdPairResponse?.Data?.ImagePath ?? string.Empty);

            // send image to the external image process service

            HttpClient httpClient = _httpClientFactory.CreateClient("facerec_service");
            MultipartFormDataContent formData = [];
            ByteArrayContent fileContent = new(imageBytes);
            fileContent.Headers.Add("Content-Type", "image/png");
            formData.Add(fileContent, "file", $"{Guid.NewGuid()}.png");
            HttpResponseMessage response = await httpClient.PostAsync("/detect_faces", formData);
            string responseBody = await response.Content.ReadAsStringAsync();
            ImageProcessResponse? imageProcessResponse = JsonSerializer.Deserialize<ImageProcessResponse>(responseBody, _jsonSerializerOptions);
            if (imageProcessResponse is null)
            {
                IError error = new ImageProcessServiceError();
                _logger.LogError(error.EventId, "{Code} {Message} ", error.ErrorCode, error.ErrorMessage);
                return Response<bool>.ErrorResult(error);
            }

            if(imageProcessResponse.Count == 0)
            {
                return Response<bool>.SuccessResult(true);
            }

            List<ImageFaceMapping> imageFaceMappings = imageProcessResponse.Faces.ConvertAll(x => new ImageFaceMapping(x.Id, imageUniqueIdPairResponse?.Data?.UniqueId ?? string.Empty));

            return await _imageProcessRepository.InsertImageFaceMappings(imageFaceMappings);
        }
        catch (Exception ex)
        {
            IError error = new ImageProcessError();
            _logger.LogError(error.EventId, ex, "{Code} {Message} ", error.ErrorCode, error.ErrorMessage);
            return Response<bool>.ErrorResult(error, ex);
        }
    }
}
