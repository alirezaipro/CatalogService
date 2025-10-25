using Catalog.Infrastructure;

namespace Catalog.Services;

public sealed class CatalogServices(
    CatalogDbContext context,
    ILogger<CatalogServices> logger
)
{
    public CatalogDbContext Context => context;

    public ILogger<CatalogServices> Logger => logger;
}