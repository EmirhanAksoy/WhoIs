using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO.Compression;
using WhoIsAPI.Contracts.Requests;

namespace WhoIsAPI.Endpoints;

public static class ImageBulkUploadEndpointExtension
{
    public static WebApplication AddImageBulkUploadEndpoint(this WebApplication app, string imageFolderPath)
    {
        app.MapPost("/image-bulk-upload", async (
            [FromForm] ImageBulkUploadRequest imageBulkUploadRequest,
            [FromServices] IDbConnection dbConnection,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {

                if (imageBulkUploadRequest.ZipFile is null || imageBulkUploadRequest.ZipFile.Length == 0)
                    return Results.BadRequest("No file uploaded.");

                if (!imageBulkUploadRequest.ZipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest("Only zip files are allowed.");

                if (!Directory.Exists(imageFolderPath))
                    Directory.CreateDirectory(imageFolderPath);


                List<string> imagePaths = [];
                using (var archive = ZipFile.OpenRead(imageBulkUploadRequest.ZipFile.FileName))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            if (IsImageFile(entry.FullName))
                            {
                                var guidFileName = Guid.NewGuid().ToString() + Path.GetExtension(entry.FullName);
                                var imagePath = Path.Combine(imageFolderPath, guidFileName);
                                entry.ExtractToFile(imagePath);
                                imagePaths.Add(imagePath);
                            }
                        }
                    }
                }
                List<ImageUniqueIdPair> images = imagePaths.ConvertAll(imagePath => new ImageUniqueIdPair(Guid.NewGuid().ToString(), imagePath));

                await SaveImages(images,dbConnection);

                return Results.Ok(images);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the request.");
                return Results.Problem("An error occurred while processing the request.", null, 500, "Internal Server Error");
            }
        })
        .DisableAntiforgery()
        .WithName("Image Bulk Upload With Zip File")
        .WithOpenApi();
        return app;
    }

    private static bool IsImageFile(string fileName)
    {
        var extensions = new[] { ".jpg", ".jpeg", ".png" };
        return extensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task SaveImages(List<ImageUniqueIdPair> images, IDbConnection dbConnection)
    {
        using IDbTransaction transaction = dbConnection.BeginTransaction();
        await dbConnection.ExecuteAsync(@"
                INSERT INTO  Images (UniqueId, ImagePath)
                VALUES(@UniqueId, @ImagePath)", images, transaction: transaction);
        transaction.Commit();
    }

    public record ImageUniqueIdPair(string UniqueId, string ImagePath);
}
