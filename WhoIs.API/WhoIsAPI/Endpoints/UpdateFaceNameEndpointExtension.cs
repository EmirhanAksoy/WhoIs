using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Endpoints;

public static class UpdateFaceNameEndpointExtension
{
    public static WebApplication AddUpdateFaceNameEndpoint(this WebApplication app)
    {
        app.MapPut("/update-face-name/{imageId}/{imageName}", async (
            [FromServices] IImageService imageProcessService,
            [FromRoute] string imageId,
            [FromRoute] string imageName,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                if (string.IsNullOrEmpty(imageId))
                {
                    return Results.BadRequest("Image id cannot be null or empty");
                }
                if (string.IsNullOrEmpty(imageName))
                {
                    return Results.BadRequest("Image name cannot be null or empty");
                }

                Response<bool> serviceResponse = await imageProcessService.UpdateFaceName(imageId, imageName);
                if (!serviceResponse.Success)
                    return Results.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = serviceResponse.ErrorCode,
                        Detail = serviceResponse.Errors.FirstOrDefault(),
                        Type = serviceResponse.ErrorCode
                    });

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get face image failed");
                return Results.Problem(title: "Get face image failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithSummary("Update Face Name")
        .WithDescription("Update face name with image id")
        .WithOpenApi();
        return app;
    }
}
