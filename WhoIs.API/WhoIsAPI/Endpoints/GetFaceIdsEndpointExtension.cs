using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Extensions;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Endpoints;

public static class GetFaceIdsEndpointExtension
{
    public static WebApplication AddGetFaceIdsEndpoint(this WebApplication app)
    {
        app.MapGet("/get-face-ids", async (
            [FromServices] IImageService imageProcessService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromServices] ILogger<Program> logger) =>
        {
            Response<List<FaceInfo>> serviceResponse = await imageProcessService.GetFaces();

            if (serviceResponse.Success)
                return Results.Ok(serviceResponse);

            return Results.Problem(serviceResponse.ToBadRequestProblemDetails(httpContextAccessor.HttpContext?.Request?.Path ?? "/get-face-ids"));
        })
        .DisableAntiforgery()
        .WithSummary("Get Face Ids")
        .WithOpenApi();
        return app;
    }
}
