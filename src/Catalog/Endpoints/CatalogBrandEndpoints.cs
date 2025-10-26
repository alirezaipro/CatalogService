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

        return app;
    }

    public static async Task<Results<Created, ValidationProblem, BadRequest<string>>> CreateBrand(
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
        {
            return TypedResults.BadRequest($"A brand with the name '{model.Brand}' already exists.");
        }

        var brand = CatalogBrand.Create(model.Brand);

        services.Context.CatalogBrands.Add(brand);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/brands/{brand.Id}");
    }

    public static async Task<Results<Created, ValidationProblem, NotFound<string>>> UpdateBrand(
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
        {
            return TypedResults.NotFound($"Brand with id {model.Id} not found.");
        }

        brand.Update(model.Brand);
        await services.Context.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/catalog/api/v1/brands/{brand.Id}");
    }
}