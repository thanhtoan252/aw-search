namespace AW.Infrastructure.Persistence;

internal sealed class ProductModelProductDescriptionCulture
{
    public int ProductModelID { get; set; }
    public int ProductDescriptionID { get; set; }
    public string CultureID { get; set; } = string.Empty;
    public ProductDescription? ProductDescription { get; set; }
}
