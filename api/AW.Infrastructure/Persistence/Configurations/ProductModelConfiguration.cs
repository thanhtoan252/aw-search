using AW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductModelConfiguration : IEntityTypeConfiguration<ProductModel>
{
    public void Configure(EntityTypeBuilder<ProductModel> builder)
    {
        builder.ToTable("ProductModel", "Production");
        builder.HasKey(e => e.ModelId);
        builder.Property(e => e.ModelId).HasColumnName("ProductModelID");
        builder.Property(e => e.Name).HasMaxLength(128).IsRequired();
        builder.Ignore(e => e.Description);
        builder.HasMany<ProductModelProductDescriptionCulture>()
            .WithOne()
            .HasForeignKey(e => e.ProductModelID);
    }
}
