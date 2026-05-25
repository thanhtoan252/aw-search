using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductDescriptionConfiguration : IEntityTypeConfiguration<ProductDescription>
{
    public void Configure(EntityTypeBuilder<ProductDescription> builder)
    {
        builder.ToTable("ProductDescription", "Production");
        builder.HasKey(e => e.ProductDescriptionID);
        builder.Property(e => e.Description).HasMaxLength(400).IsRequired();
    }
}
