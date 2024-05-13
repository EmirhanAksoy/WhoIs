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
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromServices] ILogger<Program> logger) =>
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

        })
        .DisableAntiforgery()
        .WithSummary("Single Image Process")
        .WithOpenApi();
        return app;
    }
}
