using Catalog.Infrastructure;
using MassTransit;

namespace Catalog.Services;

public sealed class CatalogServices(
    CatalogDbContext context,
    ILogger<CatalogServices> logger,
    IPublishEndpoint publishEndpoint
)
{
    public CatalogDbContext Context => context;

    public ILogger<CatalogServices> Logger => logger;

    public IPublishEndpoint PublishEndpoint => publishEndpoint;
}