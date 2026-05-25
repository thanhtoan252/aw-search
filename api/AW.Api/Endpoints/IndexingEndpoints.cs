using AW.Application.Interfaces;
using AW.Api.Extensions;
using AW.Api.Mappings;

namespace AW.Api.Endpoints;

public static class IndexingEndpoints
{
    public static IEndpointRouteBuilder MapIndexingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/indexing").WithTags("Indexing");

        group.MapGet("/status", GetStatusAsync)
            .WithName("GetIndexingStatus")
            .WithSummary("Current Elasticsearch index statistics");

        group.MapPost("/trigger", TriggerAsync)
            .WithName("TriggerIndexing")
            .WithSummary("Manually trigger a full re-index");

        return app;
    }

    private static async Task<IResult> GetStatusAsync(IIndexingService indexingService, CancellationToken ct)
    {
        var status = await indexingService.GetStatusAsync(ct);

        return status.IsSuccess
            ? Results.Ok(status.Value.ToDto())
            : Results.Problem(status.Error.ToProblemDetails());
    }

    private static IResult TriggerAsync(IIndexingTrigger trigger)
    {
        var result = trigger.TriggerIndexing();

        return result.IsSuccess
            ? Results.Accepted("/api/indexing/status", new { message = "Indexing started. Poll /api/indexing/status for progress." })
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
