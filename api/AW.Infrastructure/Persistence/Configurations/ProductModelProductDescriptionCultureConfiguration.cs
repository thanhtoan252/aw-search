using AW.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductModelProductDescriptionCultureConfiguration
    : IEntityTypeConfiguration<ProductModelProductDescriptionCulture>
{
    public void Configure(EntityTypeBuilder<ProductModelProductDescriptionCulture> builder)
    {
        builder.ToTable("ProductModelProductDescriptionCulture", "Production");
        builder.HasKey(e => new { e.ProductModelID, e.ProductDescriptionID, e.CultureID });
        builder.Property(e => e.CultureID).HasMaxLength(6).IsRequired();

        builder.HasOne(e => e.ProductDescription)
            .WithMany()
            .HasForeignKey(e => e.ProductDescriptionID);
    }
}
