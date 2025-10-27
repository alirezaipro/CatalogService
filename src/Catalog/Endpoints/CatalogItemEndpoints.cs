using Catalog.Endpoints.Contracts.CatalogItem;
using Catalog.Infrastructure.Extensions;
using Catalog.Models;
using Catalog.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Endpoints;

public static class CatalogItemEndpoints
{
    public static IEndpointRouteBuilder MapCatalogItemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateItem);
        app.MapPut("/", UpdateItem);
        app.MapPatch("/max_stock_threshold", UpdateMaxStockThreshold);
        app.MapDelete("/{slug:required}", DeleteItemById);
        app.MapGet("/{slug:required}", GetItemById);
        app.MapGet("/", GetItems);

        return app;
    }

    public static async Task<Results<Created, ValidationProblem, BadRequest<string>>> CreateItem(
        [AsParameters] CatalogServices services,
        CreateCatalogItemRequest model,
        IValidator<CreateCatalogItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = await validator.ValidateAsync(model, cancellationToken);
        if (!validate.IsValid)
            return TypedResults.ValidationProblem(validate.ToDictionary());

        var hasCategory =
            await services.Context.CatalogCategories.AnyAsync(x => x.Id == model.CatalogId, cancellationToken);
        if (!hasCategory)
            return TypedResults.BadRequest($"A category Id is not valid.");

        var hasBrand = await services.Context.CatalogBrands.AnyAsync(x => x.Id == model.BrandId, cancellationToken);
        if (!hasBrand)
            return TypedResults.BadRequest($"A brand Id is not valid.");

        var hasItemSlug =
            await services.Context.CatalogItems.AnyAsync(x => x.Slug == model.Name.ToSlug(), cancellationToken);
        if (hasItemSlug)
            return TypedResults.BadRequest($"A Item with the slug '{model.Name.ToSlug()}' already exists.");

        var item = CatalogItem.Create(
            model.Name,
            model.Description,
            model.MaxStockThreshold,
            model.BrandId,
            model.CatalogId);

        await services.Context.CatalogItems.AddAsync(item, cancellationToken);
        await services.Context.SaveChangesAsync(cancellationToken);

        var detailUrl = $"/catalog/api/v1/items/{item.Slug}";

        return TypedResults.Created(detailUrl);
    }

    public static async Task<Results<Created, ValidationProblem, NotFound<string>, BadRequest<string>>> UpdateItem(
        [AsParameters] CatalogServices services,
        UpdateCatalogItemRequest model,
        IValidator<UpdateCatalogItemRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = await validator.ValidateAsync(model, cancellationToken);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var item = await services.Context.CatalogItems
            .FirstOrDefaultAsync(i => i.Slug == model.slug, cancellationToken);
        if (item is null)
            return TypedResults.NotFound($"Item with slug {model.slug} not found.");

        var hasCategory =
            await services.Context.CatalogCategories.AnyAsync(x => x.Id == model.CatalogId, cancellationToken);
        if (!hasCategory)
            return TypedResults.BadRequest($"A category Id is not valid.");

        var hasBrand = await services.Context.CatalogBrands.AnyAsync(x => x.Id == model.BrandId, cancellationToken);
        if (!hasBrand)
            return TypedResults.BadRequest($"A brand Id is not valid.");

        item.Update(model.Description,
            model.BrandId,
            model.CatalogId);

        await services.Context.SaveChangesAsync(cancellationToken);

        var loadedItem = await services.Context.CatalogItems
            .Include(ci => ci.CatalogBrand)
            .Include(ci => ci.CatalogCategory)
            .FirstAsync(x => x.Slug == item.Slug, cancellationToken);

        var detailUrl = $"/catalog/api/v1/items/{loadedItem.Slug}";

        return TypedResults.Created(detailUrl);
    }

    public static async Task<Results<Created, ValidationProblem, NotFound<string>, BadRequest<string>>>
        UpdateMaxStockThreshold(
            [AsParameters] CatalogServices services,
            UpdateCatalogItemMaxStockThresholdRequest itemToUpdate,
            IValidator<UpdateCatalogItemMaxStockThresholdRequest> validator,
            CancellationToken cancellationToken)
    {
        var validate = validator.Validate(itemToUpdate);
        if (!validate.IsValid)
        {
            return TypedResults.ValidationProblem(validate.ToDictionary());
        }

        var Item = await services.Context.CatalogItems.FirstOrDefaultAsync(i => i.Slug == itemToUpdate.Slug,
            cancellationToken);
        if (Item is null)
        {
            return TypedResults.NotFound($"Item with Slug {itemToUpdate.Slug} not found.");
        }

        Item.SetMaxStockThreshold(itemToUpdate.MaxStockThreshold);

        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/items/{Item.Slug}");
    }

    public static async Task<Results<NoContent, NotFound, BadRequest<string>>> DeleteItemById(
        [AsParameters] CatalogServices services,
        string slug,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(slug))
            return TypedResults.BadRequest("Slug is not valid.");

        var item = await services.Context.CatalogItems.FirstOrDefaultAsync(x => x.Slug == slug,
            cancellationToken: cancellationToken);
        if (item is null)
            return TypedResults.NotFound();

        services.Context.CatalogItems.Remove(item);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<CatalogItemResponse>, NotFound, BadRequest<string>>> GetItemById(
        [AsParameters] CatalogServices services,
        string slug,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(slug))
            return TypedResults.BadRequest("Slug is not valid.");

        var item = await services.Context.CatalogItems
            .Include(x => x.CatalogBrand)
            .Include(x => x.CatalogCategory)
            .Include(x => x.Medias)
            .FirstOrDefaultAsync(ci => ci.Slug == slug, cancellationToken);
        if (item is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(
            new CatalogItemResponse(
                item.Name,
                item.Slug,
                item.Description,
                item.CatalogBrandId,
                item.CatalogBrand.Brand,
                item.CatalogCategoryId,
                item.CatalogCategory.Category,
                item.Price,
                item.AvailableStock,
                item.MaxStockThreshold, [.. item.Medias]));
    }

    public static async Task<Results<Ok<IEnumerable<CatalogItemResponse>>, BadRequest<string>>> GetItems(
        [AsParameters] CatalogServices services,
        CancellationToken cancellationToken)
    {
        var items = await services.Context.CatalogItems
            .Include(x => x.CatalogBrand)
            .Include(x => x.CatalogCategory)
            .Select(x => new CatalogItemResponse(x.Name,
                x.Slug,
                x.Description,
                x.CatalogBrandId,
                x.CatalogBrand.Brand,
                x.CatalogCategoryId,
                x.CatalogCategory.Category,
                x.Price,
                x.AvailableStock,
                x.MaxStockThreshold,
                x.Medias.ToList()))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(items.AsEnumerable());
    }
}