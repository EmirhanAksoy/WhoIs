using Microsoft.AspNetCore.Mvc;
using Serilog;
using WhoIsAPI.Contracts.Requests;
using WhoIsAPI.Contracts.Responses;

const string sharedDirectoryPath = "shared_data";
const string faceRecognizeServiceKey = "facerec_service";


var builder = WebApplication.CreateBuilder(args);

string faceRecognizeServiceBaseURL = builder.Configuration.GetSection("FaceRecogService").Value ?? string.Empty;
string seqServerBaseURL = builder.Configuration.GetSection("SeqServer").Value ?? string.Empty; 


builder.Host.UseSerilog();
Serilog.ILogger logger = new LoggerConfiguration()
        .WriteTo
        .Seq(
          seqServerBaseURL,
          Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
Log.Logger = logger;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(faceRecognizeServiceKey, configuration =>
{
    configuration.BaseAddress = new Uri(faceRecognizeServiceBaseURL);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


if (!Directory.Exists(sharedDirectoryPath))
{
    Directory.CreateDirectory(sharedDirectoryPath);
    Console.WriteLine($"Directory '{sharedDirectoryPath}' created.");
}


app.MapPost("/register-known-face", async ([FromForm] UploadFileRequest uploadFileRequest, 
    [FromServices] IHttpClientFactory httpClientFactory,
    [FromServices] ILogger<Program> logger) =>
{
    if (uploadFileRequest?.File is null || uploadFileRequest.File.Length == 0)
        throw new ArgumentException("File cannot be nul or empty");

    if (string.IsNullOrEmpty(uploadFileRequest?.Person))
        throw new ArgumentException("Person cannot be nul or empty");

    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(uploadFileRequest.File.FileName);
    string imagePath = Path.Combine(sharedDirectoryPath, imageName);

    try
    {
        using FileStream fileStream = new(imagePath, FileMode.Create);
        await uploadFileRequest.File.CopyToAsync(fileStream);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to save file.", ex);
    }

    try
    {
        using HttpClient httpClient = httpClientFactory.CreateClient(faceRecognizeServiceKey);
        using MultipartFormDataContent formData = new MultipartFormDataContent();
        byte[] fileBytes = await File.ReadAllBytesAsync(imagePath);
        formData.Add(new ByteArrayContent(fileBytes), "file", uploadFileRequest.File.FileName);
        HttpResponseMessage response = await httpClient.PostAsync($"/faces?id={uploadFileRequest.Person}", formData);
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Response from face recognition service:");
            logger.LogInformation(responseContent);
            return Results.BadRequest(new UploadFileResponse()
            {
                FilePath = null,
                Success = false,
                ExternalAPIResponse = responseContent
            });
        }

        return Results.Ok(new UploadFileResponse()
        {
            FilePath = imagePath,
            Success = true,
            ExternalAPIResponse = responseContent
        });
    }
    catch (HttpRequestException ex)
    {
        throw new InvalidOperationException("Failed to connect to face recognition service.", ex);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("An error occurred during file upload and recognition.", ex);
    }
})
.DisableAntiforgery()
.WithName("Register Known Face")
.WithOpenApi();


app.MapPost("/identity-faces", async ([FromForm] IdentityFacesOnImageRequest identityFacesOnImageRequest, [FromServices] IHttpClientFactory httpClientFactory) =>
{
    if (identityFacesOnImageRequest?.File is null || identityFacesOnImageRequest.File.Length == 0)
        throw new ArgumentException("File cannot be nul or empty");

    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(identityFacesOnImageRequest.File.FileName);
    string imagePath = Path.Combine(sharedDirectoryPath, imageName);

    try
    {
        using FileStream fileStream = new(imagePath, FileMode.Create);
        await identityFacesOnImageRequest.File.CopyToAsync(fileStream);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Failed to save file.", ex);
    }

    try
    {
        using HttpClient httpClient = httpClientFactory.CreateClient(faceRecognizeServiceKey);
        using MultipartFormDataContent formData = new MultipartFormDataContent();
        byte[] fileBytes = await File.ReadAllBytesAsync(imagePath);
        formData.Add(new ByteArrayContent(fileBytes), "file", identityFacesOnImageRequest.File.FileName);
        HttpResponseMessage response = await httpClient.PostAsync("/", formData);
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Response from face recognition service:");
            Console.WriteLine(responseContent);
            return Results.BadRequest(new IdentityFacesOnImageResponse()
            {
                Success = false,
                ExternalAPIResponse = responseContent
            });
        }

        return Results.Ok(new IdentityFacesOnImageResponse()
        {
            Success = true,
            ExternalAPIResponse = responseContent
        });
    }
    catch (HttpRequestException ex)
    {
        throw new InvalidOperationException("Failed to connect to face recognition service.", ex);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("An error occurred during file upload and recognition.", ex);
    }
})
.DisableAntiforgery()
.WithName("Identity Faces On Image")
.WithOpenApi();


app.MapGet("/get-file-list", () =>
{
    return Directory.GetFiles(sharedDirectoryPath);
})
.WithName("Get File List")
.WithOpenApi();




app.Run();

