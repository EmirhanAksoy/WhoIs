using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Endpoints;

public static class GetImageIdsByFaceNameEndpointExtension
{
    public static WebApplication AddGetImageIdsByFaceNameEndpoint(this WebApplication app)
    {
        app.MapGet("/imageIds/face/{faceNameSearchText}", async (
            [FromServices] IImageService imageProcessService,
            [FromRoute] string faceNameSearchText,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                if (string.IsNullOrEmpty(faceNameSearchText))
                {
                    return Results.Ok(Response<List<string>>.SuccessResult([]));
                }

                Response<List<string>> serviceResponse = await imageProcessService.GetImageIdsByFaceName(faceNameSearchText);
                if (!serviceResponse.Success)
                    return Results.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = serviceResponse.ErrorCode,
                        Detail = serviceResponse.Errors.FirstOrDefault(),
                        Type = serviceResponse.ErrorCode
                    });

                return Results.Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get image ids with face name failed");
                return Results.Problem(title: "Get image ids with face name failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithSummary("Get Image Ids With Face Name")
        .WithDescription("Get image ids with face name search text")
        .WithOpenApi();
        return app;
    }
}
