namespace AW.Domain.Models;

public sealed record IndexStats(string IndexName, long DocumentCount, long? SizeBytes, bool IsHealthy);
