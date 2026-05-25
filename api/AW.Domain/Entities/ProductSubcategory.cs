namespace AW.Domain.Entities;

public sealed class ProductSubcategory
{
    public int SubcategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ProductCategory? Category { get; init; }
}
