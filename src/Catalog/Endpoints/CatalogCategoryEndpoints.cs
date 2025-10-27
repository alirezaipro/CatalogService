using Catalog.Endpoints.Contracts.CatalogCategory;
using Catalog.Models;
using Catalog.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Endpoints;

public static class CatalogCategoryEndpoints
{
    public static IEndpointRouteBuilder MapCatalogCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateCategory);
        app.MapPut("/", UpdateCategory);
        app.MapDelete("/{id:int:required}", DeleteCategoryById);
        app.MapGet("/{id:int:required}", GetCategoryById);
        app.MapGet("/", GetCategories);

        return app;
    }

    private static async Task<Results<Created, ValidationProblem, BadRequest<string>>> CreateCategory(
        [AsParameters] CatalogServices services,
        CreateCatalogCategoryRequest model,
        IValidator<CreateCatalogCategoryRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = await validator.ValidateAsync(model, cancellationToken);
        if (!validate.IsValid)
            return TypedResults.ValidationProblem(validate.ToDictionary());

        if (model.ParentId.HasValue)
        {
            var hasParent =
                await services.Context.CatalogCategories.AnyAsync(x => x.Id == model.ParentId, cancellationToken);
            if (!hasParent)
                return TypedResults.BadRequest($"A parent Id is not valid.");
        }

        var hasCategory = await services.Context.CatalogCategories.AnyAsync(x => x.Category == model.Category &&
                x.ParentId == model.ParentId,
            cancellationToken);

        if (hasCategory)
            return TypedResults.BadRequest(
                $"A Category with the name '{model.Category}' in this level already exists.");

        var category = CatalogCategory.Create(model.Category, model.ParentId);

        await services.Context.CatalogCategories.AddAsync(category, cancellationToken);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/categories/{category.Id}");
    }

    public static async Task<Results<Created, ValidationProblem, NotFound<string>>> UpdateCategory(
        [AsParameters] CatalogServices services,
        UpdateCatalogCategoryRequest model,
        IValidator<UpdateCatalogCategoryRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = await validator.ValidateAsync(model, cancellationToken);
        if (!validate.IsValid)
            return TypedResults.ValidationProblem(validate.ToDictionary());

        var category =
            await services.Context.CatalogCategories.FirstOrDefaultAsync(i => i.Id == model.Id, cancellationToken);
        if (category is null)
            return TypedResults.NotFound($"Category with id {model.Id} not found.");

        category.Update(model.Category);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/categories/{category.Id}");
    }

    public static async Task<Results<NoContent, NotFound, BadRequest<string>>> DeleteCategoryById
        ([AsParameters] CatalogServices services, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return TypedResults.BadRequest("Id is not valid.");

        var category = await services.Context.CatalogCategories
            .Include(ci => ci.Children)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
            return TypedResults.NotFound();

        if (category.Children.Any())
            return TypedResults.BadRequest("The category has child categories and cannot be deleted.");

        services.Context.CatalogCategories.Remove(category);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<CatalogCategoryResponse>, NotFound, BadRequest<string>>> GetCategoryById(
        [AsParameters] CatalogServices services,
        int id,
        CancellationToken cancellationToken
    )
    {
        if (id <= 0)
            return TypedResults.BadRequest("Id is not valid.");

        var category = await services.Context.CatalogCategories
            .Include(ci => ci.Parent)
            .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
        if (category is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(new CatalogCategoryResponse(id, category.Category, category.Path));
    }

    public static async Task<Results<Ok<IEnumerable<CatalogCategoryResponse>>, BadRequest<string>>> GetCategories(
        [AsParameters] CatalogServices services,
        CancellationToken cancellationToken)
    {
        var categories = await services.Context.CatalogCategories
            .OrderBy(c => c.Id)
            .Select(x => new CatalogCategoryResponse(x.Id, x.Category, x.Path))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IEnumerable<CatalogCategoryResponse>>(categories);
    }
}