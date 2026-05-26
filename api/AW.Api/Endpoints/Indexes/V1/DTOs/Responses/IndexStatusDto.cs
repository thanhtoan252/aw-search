namespace AW.Api.Endpoints.Indexes.V1.DTOs.Responses;

public sealed record IndexStatusDto(
    string IndexName,
    long DocumentCount,
    long? SizeBytes,
    bool IsHealthy
);
