using AW.Domain.Entities;
using AW.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AW.Infrastructure.Persistence;

internal sealed class AdventureWorksDbContext(DbContextOptions<AdventureWorksDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSubcategory> ProductSubcategories => Set<ProductSubcategory>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductModel> ProductModels => Set<ProductModel>();
    public DbSet<ProductDescription> ProductDescriptions => Set<ProductDescription>();
    public DbSet<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures => Set<ProductModelProductDescriptionCulture>();
    public DbSet<ProductPhoto> ProductPhotos => Set<ProductPhoto>();
    public DbSet<ProductProductPhoto> ProductProductPhotos => Set<ProductProductPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdventureWorksDbContext).Assembly);
    }
}
