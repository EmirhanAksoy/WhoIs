using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using WhoIsAPI.Application.Services.ImageService;
using WhoIsAPI.Endpoints;
using WhoIsAPI.Persistence.Repositories.ImageRepository;
using WhoIsAPI.Workers;


var builder = WebApplication.CreateBuilder(args);

string faceRecognizeServiceBaseURL = builder.Configuration.GetSection("FaceRecogService").Value ?? string.Empty;
string seqServerBaseURL = builder.Configuration.GetSection("SeqServer").Value ?? string.Empty;
string connectionString = builder.Configuration.GetConnectionString("WHOIS_DB") ?? string.Empty;
string faceRecognizeServiceKey = builder.Configuration.GetSection("FaceRecognitionServiceKey").Value ?? string.Empty;

builder.Host.UseSerilog();
Serilog.ILogger logger = new LoggerConfiguration()
        .WriteTo
        .Seq(
          seqServerBaseURL,
          Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
Log.Logger = logger;

builder.Services.AddHttpContextAccessor();
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

builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IImageRepository, ImageRepository>();


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
            .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error is not null)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "An error occurred while processing your request",
                Detail = exceptionHandlerPathFeature.Error.Message,
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    });
});

app.AddImageBulkUploadEndpoint();

app.AddProcessSingleImageEndpoint();

app.AddGetFaceIdsEndpoint();

app.AddGetFaceImageEndpoint();

app.AddUpdateFaceNameEndpoint();

app.AddGetImageIdsByFaceNameEndpoint();

app.Run();

