namespace AW.Domain.Entities;

public sealed class Product
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProductNumber { get; init; } = string.Empty;
    public string? Color { get; init; }
    public decimal ListPrice { get; init; }
    public decimal StandardCost { get; init; }
    public string? Size { get; init; }
    public decimal? Weight { get; init; }
    public string? ProductLine { get; init; }
    public string? Class { get; init; }
    public string? Style { get; init; }
    public DateTime SellStartDate { get; init; }
    public DateTime? DiscontinuedDate { get; init; }
    public DateTime ModifiedDate { get; init; }

    public bool IsDiscontinued => DiscontinuedDate.HasValue;

    public byte[]? ThumbnailPhoto { get; init; }

    public ProductSubcategory? Subcategory { get; init; }
    public ProductModel? Model { get; init; }
}
