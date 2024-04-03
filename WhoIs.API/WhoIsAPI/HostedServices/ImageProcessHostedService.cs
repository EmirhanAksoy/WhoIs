using System.Data;
namespace WhoIsAPI.Workers;

public class ImageProcessHostedService : IHostedService, IDisposable
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<ImageProcessHostedService> _logger;
    private readonly int DELAY_IN_SEC = 3;
    private Timer? _timer = null;

    public ImageProcessHostedService(ILogger<ImageProcessHostedService> logger, IDbConnection dbConnection)
    {
        _logger = logger;
        _dbConnection = dbConnection;
    }

    public void Dispose()
    {
        _logger.LogInformation("{HostedService} is disposing", nameof(ImageProcessHostedService));
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{HostedService} is running", nameof(ImageProcessHostedService));

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(DELAY_IN_SEC));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{HostedService} is stopping", nameof(ImageProcessHostedService));

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
}
