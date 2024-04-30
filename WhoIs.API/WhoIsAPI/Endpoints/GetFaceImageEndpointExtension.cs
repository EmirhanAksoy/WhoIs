using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageProcess;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Endpoints;

public static class GetFaceImageEndpointExtension
{
    public static WebApplication AddGetFaceImageEndpoint(this WebApplication app)
    {
        app.MapPost("/get-face-image/{imageId}", async (
            [FromServices] IImageProcessService imageProcessService,
            [FromRoute] string imageId,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                Response<string> facePathResponse = await imageProcessService.GetFaceImagePath(imageId);
                if (string.IsNullOrEmpty(facePathResponse?.Data))
                {
                    return Results.NotFound($"Face image not found with given image id {imageId}");
                }
                string imagePath = facePathResponse.Data.Replace(@"/root", ".");
                if (!File.Exists(imagePath))
                {
                    logger.LogInformation("File not exists with {path}", facePathResponse.Data);
                }
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                return Results.File(imageBytes, "image/jpeg");
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get face image failed");
                return Results.Problem(title: "Get face image failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithName("Get Face Image")
        .WithOpenApi();
        return app;
    }
}
