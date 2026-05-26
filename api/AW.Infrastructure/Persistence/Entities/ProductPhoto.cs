namespace AW.Infrastructure.Persistence.Entities;

internal sealed class ProductPhoto
{
    public int ProductPhotoID { get; set; }
    public byte[]? ThumbNailPhoto { get; set; }
    public string? ThumbnailPhotoFileName { get; set; }
}
