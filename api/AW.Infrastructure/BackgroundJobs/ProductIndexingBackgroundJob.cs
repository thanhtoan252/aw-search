using AW.Application.Interfaces;
using AW.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace AW.Infrastructure.BackgroundJobs;

public sealed class ProductIndexingBackgroundJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ProductIndexingBackgroundJob> logger) : BackgroundService, IIndexingTrigger
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(configuration.GetValue("Indexing:IntervalMinutes", 60));

    // Capacity 1 + DropWrite: a second trigger while one is queued is silently ignored
    private readonly Channel<bool> _triggerChannel = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public Result<bool> TriggerIndexing() =>
        _triggerChannel.Writer.TryWrite(true)
            ? true
            : Result<bool>.Failure(Error.Conflict("Indexing.AlreadyQueued", "Indexing is already queued."));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        await EnsureIndexAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunScheduledIndexingAsync(stoppingToken);
            await WaitForNextRunAsync(stoppingToken);
        }
    }

    private async Task EnsureIndexAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var searchStore = scope.ServiceProvider.GetRequiredService<IProductSearchStore>();
        await searchStore.EnsureIndexExistsAsync(ct);
    }

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var indexingService = scope.ServiceProvider.GetRequiredService<IIndexingService>();
            await indexingService.RunIndexingAsync(ct);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Indexing job cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Indexing job threw unhandled exception");
        }
    }

    private async Task RunScheduledIndexingAsync(CancellationToken ct)
    {
        logger.LogInformation("Product indexing job triggered");
        await RunAsync(ct);
    }

    private async Task WaitForNextRunAsync(CancellationToken stoppingToken)
    {
        using var timeoutCts = CreateIntervalCancellationToken(stoppingToken);

        try
        {
            await WaitForManualTriggerAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            // Interval elapsed; next loop performs the scheduled re-index.
        }
    }

    private CancellationTokenSource CreateIntervalCancellationToken(CancellationToken stoppingToken)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(_interval);

        return timeoutCts;
    }

    private async Task WaitForManualTriggerAsync(CancellationToken ct)
    {
        await _triggerChannel.Reader.WaitToReadAsync(ct);
        _triggerChannel.Reader.TryRead(out _);
        logger.LogInformation("Manual re-index triggered via API");
    }
}
