using Asp.Versioning;
using AW.Api.Endpoints.Indexes.V1;
using AW.Api.Endpoints.Products.V1;

namespace AW.Api.Extensions;

public static class ApiEndpointExtensions
{
    private static readonly ApiVersion V1 = new(1.0);

    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(V1)
            .ReportApiVersions()
            .Build();

        var v1 = app.MapGroup("/api")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(V1);

        v1.MapProductSearchEndpoints();
        v1.MapIndexingEndpoints();

        return app;
    }
}
