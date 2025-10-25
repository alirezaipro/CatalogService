using System.Reflection;
using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    private const string DefaultSchema = "catalog";
    public const string DefaultConnectionStringName = "SvcDbContext";

    public DbSet<CatalogBrand> CatalogBrands => Set<CatalogBrand>();
    // public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    // public DbSet<CatalogCategory> CatalogCategories => Set<CatalogCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DefaultSchema);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}