using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductProductPhotoConfiguration : IEntityTypeConfiguration<ProductProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductProductPhoto> builder)
    {
        builder.ToTable("ProductProductPhoto", "Production");
        builder.HasKey(e => new { e.ProductID, e.ProductPhotoID });
        builder.Property(e => e.Primary).HasColumnName("Primary");

        builder.HasOne(e => e.ProductPhoto)
            .WithMany()
            .HasForeignKey(e => e.ProductPhotoID);
    }
}
