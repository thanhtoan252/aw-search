using AW.Domain.Entities;
using AW.Domain.Common;
using AW.Domain.Models;

namespace AW.Application.Interfaces;

public interface IProductSearchStore
{
    Task EnsureIndexExistsAsync(CancellationToken ct = default);

    Task BulkIndexAsync(IReadOnlyList<Product> products, CancellationToken ct = default);

    Task<Result<SearchResponse>> SearchAsync(SearchFilter filter, CancellationToken ct = default);

    Task<Result<ProductSearchResult>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<byte[]>> GetThumbnailAsync(int id, CancellationToken ct = default);

    Task<Result<IndexStats>> GetStatsAsync(CancellationToken ct = default);
}
