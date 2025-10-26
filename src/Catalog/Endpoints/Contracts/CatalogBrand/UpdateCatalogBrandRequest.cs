using FluentValidation;

namespace Catalog.Endpoints.Contracts.CatalogBrand;

public sealed record UpdateCatalogBrandRequest(int Id, string Brand);

public sealed class UpdateCatalogBrandRequestValidator : AbstractValidator<UpdateCatalogBrandRequest>
{
    public UpdateCatalogBrandRequestValidator()
    {
        RuleFor(i => i.Brand)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(i => i.Id)
            .NotNull();
    }
}