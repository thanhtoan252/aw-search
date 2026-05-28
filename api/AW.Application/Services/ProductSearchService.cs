using AW.Application.Interfaces;
using AW.Domain.Common;
using AW.Domain.Models;
using Microsoft.Extensions.Logging;

namespace AW.Application.Services;

public sealed class ProductSearchService(
    IProductSearchStore searchStore,
    IProductRepository productRepository,
    ILogger<ProductSearchService> logger) : IProductSearchService
{
    public Task<Result<SearchResponse>> SearchAsync(SearchFilter filter, CancellationToken ct = default)
    {
        logger.LogDebug("Searching products with filter: {@Filter}", filter);
        return searchStore.SearchAsync(filter, ct);
    }

    public Task<Result<ProductSearchResult>> GetByIdAsync(int id, CancellationToken ct = default) =>
        searchStore.GetByIdAsync(id, ct);

    public async Task<Result<byte[]>> GetThumbnailAsync(int id, CancellationToken ct = default)
    {
        var thumbnail = await productRepository.GetThumbnailAsync(id, ct);

        return thumbnail is { Length: > 0 }
            ? Result<byte[]>.Success(thumbnail)
            : Result<byte[]>.Failure(Error.NotFound(
                "Products.ThumbnailNotFound",
                $"Thumbnail for product {id} was not found."));
    }
}
