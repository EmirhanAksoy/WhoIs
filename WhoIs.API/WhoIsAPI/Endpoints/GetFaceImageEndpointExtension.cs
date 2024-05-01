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
                Response<string> serviceResponse = await imageProcessService.GetFaceImagePath(imageId);
                if (!serviceResponse.Success)
                    return Results.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = serviceResponse.ErrorCode,
                        Detail = serviceResponse.Errors.FirstOrDefault(),
                        Type = serviceResponse.ErrorCode
                    });
                if (string.IsNullOrEmpty(serviceResponse?.Data))
                {
                    return Results.NotFound($"Face image not found with given image id {imageId}");
                }
                string imagePath = serviceResponse.Data;
                if (!File.Exists(imagePath))
                {
                    logger.LogInformation("File not exists with {path}", serviceResponse.Data);
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
