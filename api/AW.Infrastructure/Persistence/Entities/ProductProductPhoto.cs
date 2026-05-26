namespace AW.Infrastructure.Persistence.Entities;

internal sealed class ProductProductPhoto
{
    public int ProductID { get; set; }
    public int ProductPhotoID { get; set; }
    public bool Primary { get; set; }
    public ProductPhoto? ProductPhoto { get; set; }
}
