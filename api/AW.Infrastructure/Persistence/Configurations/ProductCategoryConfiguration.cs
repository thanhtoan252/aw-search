using AW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategory", "Production");
        builder.HasKey(e => e.CategoryId);
        builder.Property(e => e.CategoryId).HasColumnName("ProductCategoryID");
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
    }
}
