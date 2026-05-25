using AW.Domain.Common;
using AW.Domain.Models;

namespace AW.Application.Interfaces;

public interface IIndexingService
{
    Task RunIndexingAsync(CancellationToken ct = default);

    Task<Result<IndexStats>> GetStatusAsync(CancellationToken ct = default);
}
