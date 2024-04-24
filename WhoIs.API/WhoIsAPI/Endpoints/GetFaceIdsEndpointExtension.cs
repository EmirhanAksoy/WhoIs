using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageProcess;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Models;

namespace WhoIsAPI.Endpoints;

public static class GetFaceIdsEndpointExtension
{
    public static WebApplication AddGetFaceIdsEndpoint(this WebApplication app)
    {
        app.MapPost("/get-face-ids", async (
            [FromServices] IImageProcessService imageProcessService,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                Response<List<FaceInfo>> serviceResponse = await imageProcessService.GetFaces();
                if (serviceResponse.Success)
                    return Results.Ok(serviceResponse);

                return Results.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = serviceResponse.ErrorCode,
                    Detail = serviceResponse.Errors.FirstOrDefault(),
                    Type = serviceResponse.ErrorCode
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get faces failed");
                return Results.Problem(title: "Get faces failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithName("Get Face Ids")
        .WithOpenApi();
        return app;
    }
}
