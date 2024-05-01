using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Endpoints;

public static class ProcessSingleImageEndpointExtension
{
    public static WebApplication AddProcessSingleImageEndpoint(this WebApplication app)
    {
        app.MapPost("/process-single-image", async (
            [FromServices] IImageService imageProcessService,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                Response<bool> serviceResponse = await imageProcessService.ProcessImages();
                if (serviceResponse.Success)
                    return Results.Ok();

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
                logger.LogError(ex, "Single image process failed");
                return Results.Problem(title: "Single image process failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithSummary("Single Image Process")
        .WithOpenApi();
        return app;
    }
}
