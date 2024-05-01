using Microsoft.AspNetCore.Mvc;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Contracts.Requests;
using WhoIsAPI.Domain;

namespace WhoIsAPI.Endpoints;

public static class ImageBulkUploadEndpointExtension
{
    public static WebApplication AddImageBulkUploadEndpoint(this WebApplication app)
    {
        app.MapPost("/image-bulk-upload", async (
            [FromForm] ImageBulkUploadRequest imageBulkUploadRequest,
            [FromServices] IImageService imageUploadService,
            [FromServices] IConfiguration configuration,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                if (imageBulkUploadRequest.ZipFile is null || imageBulkUploadRequest.ZipFile.Length == 0)
                    return Results.BadRequest("No file uploaded.");

                if (!imageBulkUploadRequest.ZipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest("Only zip files are allowed.");


                string imageFolderPath = configuration.GetSection("ImageFolderPath").Value ?? string.Empty;

                Response<bool> serviceResponse = await imageUploadService.UploadImages(imageBulkUploadRequest.ZipFile, imageFolderPath);

                if (!serviceResponse.Success)
                    return Results.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = serviceResponse.ErrorCode,
                        Detail = serviceResponse.Errors.FirstOrDefault(),
                        Type = serviceResponse.ErrorCode
                    });
                return Results.Created();
            }
            catch (Exception ex)
            {
                return Results.Problem(title: "Image bulk upload failed", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .DisableAntiforgery()
        .WithSummary("Image Bulk Upload With Zip File")
        .WithOpenApi();
        return app;
    }
}
