using FluentValidation;
using Microsoft.EntityFrameworkCore;


public class ProductCreateValidator : AbstractValidator<ProductRequest>
{

    public ProductCreateValidator()
    {

        RuleFor(p => p.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock must be a non-negative integer.");

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.")
            .WithMessage("Category ID does not exist.");
    }
}

