using AW.Domain.Entities;

namespace AW.Application.Interfaces;

public interface IProductRepository
{
    Task<int> CountAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetPagedAsync(int skip, int take, CancellationToken ct = default);
}
