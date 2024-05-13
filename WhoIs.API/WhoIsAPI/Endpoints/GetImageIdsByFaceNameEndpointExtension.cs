using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Extensions;

namespace WhoIsAPI.Endpoints;

public static class GetImageIdsByFaceNameEndpointExtension
{
    public static WebApplication AddGetImageIdsByFaceNameEndpoint(this WebApplication app)
    {
        app.MapGet("/imageIds/face/{faceNameSearchText}", async (
            [FromServices] IImageService imageProcessService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromRoute] string faceNameSearchText,
            [FromServices] ILogger<Program> logger) =>
        {

            if (string.IsNullOrEmpty(faceNameSearchText))
                return Results.Ok(Response<List<string>>.SuccessResult([]));

            Response<List<string>> serviceResponse = await imageProcessService.GetImageIdsByFaceName(faceNameSearchText);

            if (!serviceResponse.Success)
                return Results.Problem(serviceResponse.ToBadRequestProblemDetails(httpContextAccessor.HttpContext?.Request.Path ?? "/imageIds/face/{faceNameSearchText}"));

            return Results.Ok(serviceResponse);

        })
        .DisableAntiforgery()
        .WithSummary("Get Image Ids With Face Name")
        .WithDescription("Get image ids with face name search text")
        .WithOpenApi();
        return app;
    }
}
