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

        var photoByProductId = await LoadPhotosByProductIdAsync(products, ct);
        var descriptionByModelId = await LoadDescriptionsByModelIdAsync(products, ct);

        return [.. products.Select(p => HydrateProduct(p, photoByProductId, descriptionByModelId))];
    }

    private Task<List<Product>> LoadProductPageAsync(int skip, int take, CancellationToken ct) =>
        db.Products
            .AsNoTracking()
            .Include(p => p.Subcategory!.Category)
            .Include(p => p.Model)
            .OrderBy(p => p.ProductId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    private async Task<Dictionary<int, byte[]?>> LoadPhotosByProductIdAsync(
        IReadOnlyList<Product> products,
        CancellationToken ct)
    {
        var productIds = products.Select(p => p.ProductId).ToList();

        return (await db.ProductProductPhotos
                .AsNoTracking()
                .Where(pp => productIds.Contains(pp.ProductID))
                .Include(pp => pp.ProductPhoto)
                .ToListAsync(ct))
            .GroupBy(pp => pp.ProductID)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(pp => pp.Primary).First().ProductPhoto?.ThumbNailPhoto);
    }

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
        IReadOnlyDictionary<int, byte[]?> photoByProductId,
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
        ThumbnailPhoto = photoByProductId.GetValueOrDefault(product.ProductId),
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
