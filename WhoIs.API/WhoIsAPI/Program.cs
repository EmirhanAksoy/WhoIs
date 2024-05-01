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

app.AddImageBulkUploadEndpoint();

app.AddProcessSingleImageEndpoint();

app.AddGetFaceIdsEndpoint();

app.AddGetFaceImageEndpoint();

app.AddUpdateFaceNameEndpoint();

app.AddGetImageIdsByFaceNameEndpoint();

app.Run();

