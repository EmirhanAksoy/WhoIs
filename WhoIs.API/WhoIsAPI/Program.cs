using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using WhoIsAPI.Application.Services.ImageProcess;
using WhoIsAPI.Application.Services.ImageUpload;
using WhoIsAPI.Endpoints;
using WhoIsAPI.Persistence.Repositories.ImageProcessRepository;
using WhoIsAPI.Workers;

const string facesFolder = "./faces";
const string imagesFolder = "./images";
const string faceRecognizeServiceKey = "facerec_service";

var builder = WebApplication.CreateBuilder(args);

string faceRecognizeServiceBaseURL = builder.Configuration.GetSection("FaceRecogService").Value ?? string.Empty;
string seqServerBaseURL = builder.Configuration.GetSection("SeqServer").Value ?? string.Empty;
string? connectionString = builder.Configuration.GetConnectionString("WHOIS_DB");


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

builder.Services.AddTransient<IDbConnection>(_ =>
{
    return new SqlConnection(connectionString);
});

builder.Services.AddTransient<IImageUploadService, ImageUploadService>();

builder.Services.AddTransient<IImageProcessService, ImageProcessService>();
builder.Services.AddTransient<IImageProcessRepository, ImageProcessRepository>();


builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = true;
});

builder.Services.AddHostedService<ImageProcessHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.AddImageBulkUploadEndpoint(imagesFolder);

app.AddProcessSingleImageEndpoint();

app.AddGetFaceIdsEndpoint();

app.AddGetFaceImageEndpoint();

app.MapGet("/get-files/{isFaceFolder}", ([FromRoute] bool isFaceFolder) =>
{
    return Directory.GetFiles(isFaceFolder ? facesFolder : imagesFolder);
})
.WithName("Get File List")
.WithOpenApi();


app.Run();

