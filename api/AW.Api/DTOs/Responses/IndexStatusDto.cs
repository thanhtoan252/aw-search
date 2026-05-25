namespace AW.Api.DTOs.Responses;

public sealed record IndexStatusDto(
    string IndexName,
    long DocumentCount,
    long? SizeBytes,
    bool IsHealthy
);
