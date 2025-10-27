using Catalog.Endpoints.Contracts;
using Catalog.Endpoints.Contracts.CatalogBrand;
using Catalog.Models;
using Catalog.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Endpoints;

public static class CatalogBrandEndpoints
{
    public static IEndpointRouteBuilder MapCatalogBrandEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateBrand);
        app.MapPut("/", UpdateBrand);
        app.MapGet("/", GetBrands);
        app.MapGet("/{id:required:int}", GetBrandById);
        app.MapDelete("/{id:required:int}", DeleteBrandById);
        
        return app;
    }

    private static async Task<Results<Created, ValidationProblem, BadRequest<string>>> CreateBrand(
        CreateCatalogBrandRequest model,
        IValidator<CreateCatalogBrandRequest> validator,
        [AsParameters] CatalogServices services,
        CancellationToken cancellationToken
    )
    {
        var validate = await validator.ValidateAsync(model, cancellationToken);
        if (!validate.IsValid)
            return TypedResults.ValidationProblem(validate.ToDictionary());

        var hasBrand = await services.Context.CatalogBrands.AnyAsync(x => x.Brand == model.Brand, cancellationToken);
        if (hasBrand)
            return TypedResults.BadRequest($"A brand with the name '{model.Brand}' already exists.");

        var brand = CatalogBrand.Create(model.Brand);

        services.Context.CatalogBrands.Add(brand);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/brands/{brand.Id}");
    }

    private static async Task<Results<Created, ValidationProblem, NotFound<string>>> UpdateBrand(
        UpdateCatalogBrandRequest model,
        [AsParameters] CatalogServices services,
        IValidator<UpdateCatalogBrandRequest> validator,
        CancellationToken cancellationToken)
    {
        var validate = await validator.ValidateAsync(model, cancellationToken);

        if (!validate.IsValid)
            return TypedResults.ValidationProblem(validate.ToDictionary());

        var brand = await services.Context.CatalogBrands.FirstOrDefaultAsync(i => i.Id == model.Id, cancellationToken);
        if (brand is null)
            return TypedResults.NotFound($"Brand with id {model.Id} not found.");

        brand.Update(model.Brand);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/brands/{brand.Id}");
    }

    private static async Task<Results<Ok<CatalogBrandResponse>, BadRequest<string>, NotFound<string>>> GetBrandById(
        [AsParameters] CatalogServices services,
        int id)
    {
        if (id <= 0)
            return TypedResults.BadRequest("Id is not valid.");

        var brand = await services.Context.CatalogBrands.FirstOrDefaultAsync(ci => ci.Id == id);
        if (brand is null)
            return TypedResults.NotFound("Catalog not found");


        return TypedResults.Ok(new CatalogBrandResponse(id, brand.Brand));
    }
    
    public static async Task<Results<Ok<IEnumerable<CatalogBrandResponse>>, BadRequest<string>>> GetBrands(
        [AsParameters] CatalogServices services,
        CancellationToken cancellationToken)
    {
        var brands = await services.Context.CatalogBrands
            .OrderBy(c => c.Id)
            .Select(x => new CatalogBrandResponse(x.Id, x.Brand))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IEnumerable<CatalogBrandResponse>>(brands);
    }
    
    
    public static async Task<Results<NoContent, NotFound, BadRequest<string>>> DeleteBrandById
        ([AsParameters] CatalogServices services, int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return TypedResults.BadRequest("Id is not valid.");

        var brand = await services.Context.CatalogBrands.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        if (brand is null)
            return TypedResults.NotFound();

        services.Context.CatalogBrands.Remove(brand);
        await services.Context.SaveChangesAsync(cancellationToken);
        
        return TypedResults.NoContent();
    }
}