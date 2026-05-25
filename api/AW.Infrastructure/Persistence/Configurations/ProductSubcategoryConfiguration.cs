using AW.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductSubcategoryConfiguration : IEntityTypeConfiguration<ProductSubcategory>
{
    public void Configure(EntityTypeBuilder<ProductSubcategory> builder)
    {
        builder.ToTable("ProductSubcategory", "Production");
        builder.HasKey(e => e.SubcategoryId);
        builder.Property(e => e.SubcategoryId).HasColumnName("ProductSubcategoryID");
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey("ProductCategoryID");
    }
}
