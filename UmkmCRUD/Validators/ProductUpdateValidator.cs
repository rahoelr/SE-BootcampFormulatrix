using FluentValidation;

public class ProductPatchValidator : AbstractValidator<ProductRequest>
{
    public ProductPatchValidator()
    {
        // ProductName hanya divalidasi jika ada value
        When(x => x.ProductName != null, () =>
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("ProductName tidak boleh kosong")
                .MaximumLength(100).WithMessage("ProductName maksimal 100 karakter");
        });

        // Stock hanya divalidasi jika ada value
        When(x => x.Stock.HasValue, () =>
        {
            RuleFor(x => x.Stock.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Stock harus 0 atau lebih");
        });

        // CategoryId hanya divalidasi jika tidak default
        When(x => x.CategoryId != Guid.Empty, () =>
        {
            RuleFor(x => x.CategoryId)
                .NotEqual(Guid.Empty).WithMessage("CategoryId tidak boleh kosong");
        });
    }
}
