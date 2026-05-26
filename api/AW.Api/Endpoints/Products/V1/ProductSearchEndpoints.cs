using AW.Application.Interfaces;
using AW.Api.Extensions;
using AW.Api.Endpoints.Products.V1.DTOs.Requests;
using AW.Api.Endpoints.Products.V1.Mappings;
using AW.Domain.Common;
using FluentValidation;

namespace AW.Api.Endpoints.Products.V1;

public static class ProductSearchEndpoints
{
    public static IEndpointRouteBuilder MapProductSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/search", SearchAsync)
            .WithName("SearchProductsV1")
            .WithSummary("Full-text search with faceted filtering")
            .WithDescription("Supports fuzzy matching on name/number/model/category. Filters: category, color, price range.");

        group.MapGet("/{id:int}", GetByIdAsync)
            .WithName("GetProductByIdV1")
            .WithSummary("Get single product by ID from search index");

        group.MapGet("/{id:int}/thumbnail", GetThumbnailAsync)
            .WithName("GetProductThumbnailV1")
            .WithSummary("Get product thumbnail image");

        return app;
    }

    private static async Task<IResult> SearchAsync(
        [AsParameters] ProductSearchRequestDto req,
        IValidator<ProductSearchRequestDto> validator,
        IProductSearchService searchService,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await searchService.SearchAsync(ProductSearchMapper.ToFilter(req), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> GetByIdAsync(int id, IProductSearchService searchService, CancellationToken ct)
    {
        if (id <= 0)
        {
            return Results.Problem(Error
                .Validation("Products.InvalidId", "Product ID must be greater than 0.")
                .ToProblemDetails());
        }

        var result = await searchService.GetByIdAsync(id, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDto())
            : Results.Problem(result.Error.ToProblemDetails());
    }

    private static async Task<IResult> GetThumbnailAsync(int id, IProductSearchService searchService, CancellationToken ct)
    {
        if (id <= 0)
        {
            return Results.Problem(Error
                .Validation("Products.InvalidId", "Product ID must be greater than 0.")
                .ToProblemDetails());
        }

        var result = await searchService.GetThumbnailAsync(id, ct);
        
        return result.IsSuccess
            ? Results.File(result.Value, "image/gif")
            : Results.Problem(result.Error.ToProblemDetails());
    }
}
