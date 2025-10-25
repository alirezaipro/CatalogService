using Catalog.Endpoints.Contracts;
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

        return app;
    }

    public static async Task<Results<Created, ValidationProblem, BadRequest<string>>> CreateBrand(
        CreateCatalogBrandRequest model,
        IValidator<CreateCatalogBrandRequest> validator,
        [AsParameters] CatalogServices services,
        CancellationToken cancellationToken
    )
    {
        var validate = await validator.ValidateAsync(model);
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
}