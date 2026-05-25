namespace AW.Domain.Entities;

public sealed class ProductModel
{
    public int ModelId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
