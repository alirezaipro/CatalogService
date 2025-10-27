using Catalog.Models;

namespace Catalog.Endpoints.Contracts.CatalogItem;

public sealed record CatalogItemResponse(
    string Name,
    string Slug,
    string Description,
    int BrandId,
    string BrandName,
    int CategoryId,
    string CategoryName,
    decimal Price,
    int AvailableStock,
    int MaxStockThreshold,
    IReadOnlyCollection<CatalogMedia> medias);