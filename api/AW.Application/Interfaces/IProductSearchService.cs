using AW.Domain.Common;
using AW.Domain.Models;

namespace AW.Application.Interfaces;

public interface IProductSearchService
{
    Task<Result<SearchResponse>> SearchAsync(SearchFilter filter, CancellationToken ct = default);

    Task<Result<ProductSearchResult>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<byte[]>> GetThumbnailAsync(int id, CancellationToken ct = default);
}
