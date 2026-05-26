using AW.Api.Endpoints.Indexes.V1.DTOs.Responses;
using AW.Domain.Models;

namespace AW.Api.Endpoints.Indexes.V1.Mappings;

internal static class IndexingMapper
{
    internal static IndexStatusDto ToDto(this IndexStats stats) => new(
        stats.IndexName,
        stats.DocumentCount,
        stats.SizeBytes,
        stats.IsHealthy
    );
}
