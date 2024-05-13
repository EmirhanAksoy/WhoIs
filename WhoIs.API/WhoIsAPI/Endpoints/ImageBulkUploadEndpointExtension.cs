using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Contracts.Requests;
using WhoIsAPI.Domain;
using WhoIsAPI.Domain.Extensions;

namespace WhoIsAPI.Endpoints;

public static class ImageBulkUploadEndpointExtension
{
    public static WebApplication AddImageBulkUploadEndpoint(this WebApplication app)
    {
        app.MapPost("/image-bulk-upload", async (
            [FromForm] ImageBulkUploadRequest imageBulkUploadRequest,
            [FromServices] IImageService imageUploadService,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<Program> logger) =>
        {
            string instance = httpContextAccessor.HttpContext?.Request?.Path ?? "/image-bulk-upload";

            if (imageBulkUploadRequest.ZipFile is null || imageBulkUploadRequest.ZipFile.Length == 0)
            {
                return Results.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "No file uploaded",
                    Detail = "File is required",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Instance = instance
                });
            }

            if (!imageBulkUploadRequest.ZipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid file content type",
                    Detail = "Only zip files are allowed",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                    Instance = instance
                });
            }

            string imageFolderPath = configuration.GetSection("ImageFolderPath").Value ?? string.Empty;

            Response<bool> serviceResponse = await imageUploadService.UploadImages(imageBulkUploadRequest.ZipFile, imageFolderPath);

            if (!serviceResponse.Success)
                return Results.Problem(serviceResponse.ToBadRequestProblemDetails(instance));

            return Results.Created();
        })
        .DisableAntiforgery()
        .WithSummary("Image Bulk Upload With Zip File")
        .WithOpenApi();
        return app;
    }
}
