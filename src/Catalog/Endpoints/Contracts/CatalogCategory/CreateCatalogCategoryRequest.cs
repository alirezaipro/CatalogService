using FluentValidation;

namespace Catalog.Endpoints.Contracts.CatalogCategory;

public sealed record CreateCatalogCategoryRequest(string Category,int? ParentId);

public sealed class CreateCatalogCategoryRequestValidator : AbstractValidator<CreateCatalogCategoryRequest>
{
    public CreateCatalogCategoryRequestValidator()
    {
        RuleFor(i => i.Category)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(i => i.ParentId)
            .Must(i => !i.HasValue || (i.HasValue && i.Value > 0)).WithMessage("ParentId, if provided, must be a greater than zero.");
    }
}