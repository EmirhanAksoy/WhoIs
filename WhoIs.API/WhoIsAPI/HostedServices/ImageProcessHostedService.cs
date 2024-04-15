namespace WhoIsAPI.Workers;

public class ImageProcessHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageProcessHostedService> _logger;

    public ImageProcessHostedService(ILogger<ImageProcessHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private async Task ProcessImage(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{HostedService} is working", nameof(ImageProcessHostedService));

        //using var scope = _serviceProvider.CreateScope();
        //ImageProcesService imageProcesService =
        //    scope.ServiceProvider
        //        .GetRequiredService<ImageProcesService>();

        //await imageProcesService.ProcessImages();
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("{HostedService} is running", nameof(ImageProcessHostedService));

            await ProcessImage(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while processing image");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{HostedService} is stopping", nameof(ImageProcessHostedService));
        await base.StopAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _logger.LogInformation("{HostedService} is disposing", nameof(ImageProcessHostedService));
        base.Dispose();
    }
}
