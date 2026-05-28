using AW.Domain.Entities;
using AW.Domain.Models;

namespace AW.Infrastructure.Search;

internal static class ProductDocumentMapper
{
    internal static ProductDocument ToDocument(Product p) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        ProductNumber = p.ProductNumber,
        Color = p.Color,
        ListPrice = p.ListPrice,
        StandardCost = p.StandardCost,
        Size = p.Size,
        Weight = p.Weight,
        ProductLine = p.ProductLine,
        Class = p.Class,
        CategoryName = p.Subcategory?.Category?.Name,
        SubcategoryName = p.Subcategory?.Name,
        ModelName = p.Model?.Name,
        Description = p.Model?.Description,
        IsDiscontinued = p.IsDiscontinued,
        SellStartDate = p.SellStartDate,
        IndexedAt = DateTime.UtcNow,
    };

    internal static ProductSearchResult ToResult(ProductDocument doc) => new()
    {
        ProductId = doc.ProductId,
        Name = doc.Name,
        ProductNumber = doc.ProductNumber,
        Color = doc.Color,
        ListPrice = doc.ListPrice,
        Size = doc.Size,
        CategoryName = doc.CategoryName,
        SubcategoryName = doc.SubcategoryName,
        ModelName = doc.ModelName,
        Description = doc.Description,
        ProductLine = doc.ProductLine,
        IsDiscontinued = doc.IsDiscontinued
    };

}
