using AW.Domain.Entities;
using AW.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AW.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", "Production");
        builder.HasKey(e => e.ProductId);
        builder.Property(e => e.ProductId).HasColumnName("ProductID");
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ProductNumber).HasMaxLength(25).IsRequired();
        builder.Property(e => e.Color).HasMaxLength(15);
        builder.Property(e => e.Size).HasMaxLength(5);
        builder.Property(e => e.ProductLine).HasMaxLength(2);
        builder.Property(e => e.Class).HasMaxLength(2);
        builder.Property(e => e.Style).HasMaxLength(2);
        builder.Property(e => e.StandardCost).HasColumnType("money");
        builder.Property(e => e.ListPrice).HasColumnType("money");
        builder.Property(e => e.Weight).HasColumnType("decimal(8, 2)");
        builder.Ignore(e => e.IsDiscontinued);
        builder.Ignore(e => e.ThumbnailPhoto);
        builder.HasOne(e => e.Subcategory)
            .WithMany()
            .HasForeignKey("ProductSubcategoryID");
        builder.HasOne(e => e.Model)
            .WithMany()
            .HasForeignKey("ProductModelID");
        builder.HasMany<ProductProductPhoto>()
            .WithOne()
            .HasForeignKey(pp => pp.ProductID);
    }
}
