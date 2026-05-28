using AW.Application.Interfaces;
using AW.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AW.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(AdventureWorksDbContext db) : IProductRepository
{
    public Task<int> CountAsync(CancellationToken ct = default) => db.Products.CountAsync(ct);

    public async Task<IReadOnlyList<Product>> GetPagedAsync(int skip, int take, CancellationToken ct = default)
    {
        var products = await LoadProductPageAsync(skip, take, ct);

        if (products.Count == 0) return [];

        var descriptionByModelId = await LoadDescriptionsByModelIdAsync(products, ct);

        return [.. products.Select(p => HydrateProduct(p, descriptionByModelId))];
    }

    public async Task<byte[]?> GetThumbnailAsync(int productId, CancellationToken ct = default) =>
        await db.ProductProductPhotos
            .AsNoTracking()
            .Where(pp => pp.ProductID == productId)
            .Include(pp => pp.ProductPhoto)
            .OrderByDescending(pp => pp.Primary)
            .Select(pp => pp.ProductPhoto!.ThumbNailPhoto)
            .FirstOrDefaultAsync(ct);

    private Task<List<Product>> LoadProductPageAsync(int skip, int take, CancellationToken ct) =>
        db.Products
            .AsNoTracking()
            .Include(p => p.Subcategory!.Category)
            .Include(p => p.Model)
            .OrderBy(p => p.ProductId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    private Task<Dictionary<int, string?>> LoadDescriptionsByModelIdAsync(
        IReadOnlyList<Product> products,
        CancellationToken ct)
    {
        var modelIds = products
            .Where(p => p.Model != null)
            .Select(p => p.Model!.ModelId)
            .Distinct()
            .ToList();

        return db.ProductModelProductDescriptionCultures
            .AsNoTracking()
            .Where(c => modelIds.Contains(c.ProductModelID) && c.CultureID.TrimEnd() == "en")
            .Include(c => c.ProductDescription)
            .ToDictionaryAsync(c => c.ProductModelID, c => c.ProductDescription?.Description, ct);
    }

    private static Product HydrateProduct(
        Product product,
        IReadOnlyDictionary<int, string?> descriptionByModelId) => new()
    {
        ProductId = product.ProductId,
        Name = product.Name,
        ProductNumber = product.ProductNumber,
        Color = product.Color,
        ListPrice = product.ListPrice,
        StandardCost = product.StandardCost,
        Size = product.Size,
        Weight = product.Weight,
        ProductLine = product.ProductLine?.Trim(),
        Class = product.Class?.Trim(),
        Style = product.Style?.Trim(),
        SellStartDate = product.SellStartDate,
        DiscontinuedDate = product.DiscontinuedDate,
        ModifiedDate = product.ModifiedDate,
        Subcategory = product.Subcategory,
        Model = HydrateModel(product.Model, descriptionByModelId),
    };

    private static ProductModel? HydrateModel(
        ProductModel? model,
        IReadOnlyDictionary<int, string?> descriptionByModelId) =>
        model is null
            ? null
            : new ProductModel
            {
                ModelId = model.ModelId,
                Name = model.Name,
                Description = descriptionByModelId.GetValueOrDefault(model.ModelId),
            };
}
