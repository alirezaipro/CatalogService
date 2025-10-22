using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
}