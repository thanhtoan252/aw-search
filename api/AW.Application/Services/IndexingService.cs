using AW.Application.Interfaces;
using AW.Application.Options;
using AW.Domain.Common;
using AW.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AW.Application.Services;

public sealed class IndexingService(
    IProductRepository productRepository,
    IProductSearchStore searchStore,
    IOptions<IndexingOptions> options,
    ILogger<IndexingService> logger) : IIndexingService
{
    private readonly int _batchSize = options.Value.BatchSize;

    public async Task RunIndexingAsync(CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var totalCount = await productRepository.CountAsync(ct);
        LogIndexingStarted(totalCount);

        var indexed = 0;
        var failed = 0;

        for (var skip = 0; skip < totalCount; skip += _batchSize)
        {
            if (ct.IsCancellationRequested) { break; }

            var result = await IndexBatchAsync(skip, ct);
            indexed += result.Indexed;
            failed += result.Failed;
        }

        sw.Stop();
        LogIndexingCompleted(indexed, failed, sw.ElapsedMilliseconds);
    }

    public async Task<Result<IndexStats>> GetStatusAsync(CancellationToken ct = default)
    {
        return await searchStore.GetStatsAsync(ct);
    }

    private void LogIndexingStarted(int totalCount) =>
        logger.LogInformation("Starting indexing: {Total} products, batch size {Batch}", totalCount, _batchSize);

    private async Task<(int Indexed, int Failed)> IndexBatchAsync(int skip, CancellationToken ct)
    {
        var products = await productRepository.GetPagedAsync(skip, _batchSize, ct);

        try
        {
            await searchStore.BulkIndexAsync(products, ct);
            logger.LogDebug("Indexed batch offset={Skip}, count={Count}", skip, products.Count);

            return (products.Count, 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index batch at offset {Skip}", skip);

            return (0, products.Count);
        }
    }

    private void LogIndexingCompleted(int indexed, int failed, long elapsedMilliseconds) =>
        logger.LogInformation(
            "Indexing complete — indexed={Indexed}, failed={Failed}, elapsed={Elapsed}ms",
            indexed, failed, elapsedMilliseconds);
}
