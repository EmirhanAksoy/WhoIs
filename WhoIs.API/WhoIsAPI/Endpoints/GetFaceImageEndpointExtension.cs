using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Endpoints;

public static class GetFaceImageEndpointExtension
{
    public static WebApplication AddGetFaceImageEndpoint(this WebApplication app)
    {
        app.MapPost("/get-image/{imageId}", async (
            [FromServices] IImageService imageProcessService,
            [FromServices] ILogger<Program> logger,
            [FromRoute] string imageId,
            [FromQuery] bool isFaceImage = true) =>
        {
            try
            {
                if (string.IsNullOrEmpty(imageId))
                {
                    return Results.BadRequest("Image id cannot be null or empty");
                }
                
                Response<string> imagePathResponse =  await imageProcessService.GetImagePath(imageId, isFaceImage);
                if (!imagePathResponse.Success)
                    return Results.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = imagePathResponse.ErrorCode,
                        Detail = imagePathResponse.Errors.FirstOrDefault(),
                        Type = imagePathResponse.ErrorCode
                    });


                string imagePath = imagePathResponse.Data ?? string.Empty;
                if (!File.Exists(imagePath))
                {
                    logger.LogError("Face image not exists with {imageId}", imageId);
                    return Results.BadRequest($"Face image not exists with given image id {imageId}");
                }
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                return Results.File(imageBytes, "image/png");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get face image failed");
                return Results.Problem(title: "Get face image failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithSummary("Get Image")
        .WithOpenApi();
        return app;
    }
}
