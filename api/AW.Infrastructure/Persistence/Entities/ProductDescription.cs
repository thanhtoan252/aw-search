namespace AW.Infrastructure.Persistence.Entities;

internal sealed class ProductDescription
{
    public int ProductDescriptionID { get; set; }
    public string Description { get; set; } = string.Empty;
}
