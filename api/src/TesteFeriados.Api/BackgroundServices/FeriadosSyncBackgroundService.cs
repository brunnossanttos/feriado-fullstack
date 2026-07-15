using TesteFeriados.Domain.Abstractions;

namespace TesteFeriados.Api.BackgroundServices;

public sealed class FeriadosSyncBackgroundService : BackgroundService
{
    private const int DefaultIntervalHours = 24;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FeriadosSyncBackgroundService> _logger;

    public FeriadosSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<FeriadosSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = ResolveInterval();
        _logger.LogInformation(
            "BackgroundService de sincronização iniciado. Intervalo: {Hours}h.", interval.TotalHours);

        await RunSyncSafelyAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSyncSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }

        _logger.LogInformation("BackgroundService de sincronização encerrado.");
    }

    private async Task RunSyncSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IFeriadoSyncService>();
            await syncService.SynchronizeAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no ciclo de sincronização. Nova tentativa no próximo intervalo.");
        }
    }

    private TimeSpan ResolveInterval()
    {
        var hours = _configuration.GetValue<int?>("FeriadosSync:IntervalHours") ?? DefaultIntervalHours;
        if (hours <= 0)
        {
            _logger.LogWarning(
                "FeriadosSync:IntervalHours inválido ({Hours}). Usando padrão de {Default}h.",
                hours, DefaultIntervalHours);
            hours = DefaultIntervalHours;
        }

        return TimeSpan.FromHours(hours);
    }
}
