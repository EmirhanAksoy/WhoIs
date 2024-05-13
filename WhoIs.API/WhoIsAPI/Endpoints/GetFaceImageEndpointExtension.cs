using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Extensions;

namespace WhoIsAPI.Endpoints;

public static class GetFaceImageEndpointExtension
{
    public static WebApplication AddGetFaceImageEndpoint(this WebApplication app)
    {
        app.MapPost("/get-image/{imageId}", async (
            [FromServices] IImageService imageProcessService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromServices] ILogger<Program> logger,
            [FromRoute] string imageId,
            [FromQuery] bool isFaceImage = true) =>
        {

            string instance = httpContextAccessor.HttpContext?.Request?.Path ?? "/get-image/{imageId}";

            if (string.IsNullOrEmpty(imageId))
            {
                return Results.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Image id validation error",
                    Detail = "Image id cannot be null or empty",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Instance = instance
                });
            }

            Response<string> imagePathResponse = await imageProcessService.GetImagePath(imageId, isFaceImage);

            if (!imagePathResponse.Success)
                return Results.Problem(imagePathResponse.ToBadRequestProblemDetails(instance));

            string imagePath = imagePathResponse.Data ?? string.Empty;

            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                logger.LogError("Face image not exists with {imageId}", imageId);

                return Results.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Image not exists",
                    Detail = $"Face image not exists with given image id {imageId}",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Instance = instance
                });
            }

            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            return Results.File(imageBytes, "image/png");


        })
        .DisableAntiforgery()
        .WithSummary("Get Image")
        .WithOpenApi();
        return app;
    }
}
