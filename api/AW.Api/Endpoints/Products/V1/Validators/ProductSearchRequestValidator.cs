using AW.Api.Endpoints.Products.V1.DTOs.Requests;
using FluentValidation;

namespace AW.Api.Endpoints.Products.V1.Validators;

public sealed class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequestDto>
{
    public ProductSearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0")
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize cannot exceed 100")
            .When(x => x.PageSize.HasValue);

        RuleFor(x => x.Q)
            .MaximumLength(200)
            .WithMessage("Search query cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MinPrice cannot be negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("MaxPrice cannot be negative")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Custom((request, context) =>
            {
                if (request.MinPrice.HasValue && request.MaxPrice.HasValue && 
                    request.MinPrice.Value > request.MaxPrice.Value)
                {
                    context.AddFailure("MinPrice cannot be greater than MaxPrice");
                }
            });
    }
}
