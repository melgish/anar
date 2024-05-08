using Anar.Services;

namespace Anar;

internal sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IGatewayClient _gatewayClient;
    private readonly PeriodicTimer _timer;
    private readonly IInfluxService _influxService;

    public Worker(
        ILogger<Worker> logger,
        IGatewayClient gatewayClient,
        IInfluxService influxService
    )
    {
        _logger = logger;
        _gatewayClient = gatewayClient;
        _timer = new PeriodicTimer(_gatewayClient.Interval);
        _influxService = influxService;
    }

    private async Task ProcessData(CancellationToken stoppingToken) {
        var inverters = await _gatewayClient.GetInvertersAsync(stoppingToken);
        await _influxService.WriteAsync(inverters, stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try {
            _logger.LogInformation(LogEvent.PollingStarted, "Starting polling");
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessData(stoppingToken);
                await _timer.WaitForNextTickAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) {
            // These are expected
        }
        catch (Exception ex) {
            _logger.LogError(LogEvent.WorkerError, ex, "Worker error");
        }
    }
}
