using FluentValidation;

public class CategoryPatchValidator : AbstractValidator<CategoryRequest>
{
    public CategoryPatchValidator()
    {
        RuleFor(c => c.CategoryName)
            .NotEmpty().WithMessage("Nama kategori wajib diisi")
            .MinimumLength(3).WithMessage("Nama kategori minimal 3 karakter")
            .When(c => c.CategoryName != null);

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Deskripsi wajib diisi")
            .MinimumLength(10).WithMessage("Deskripsi minimal 10 karakter")
            .When(c => c.Description != null);
    }
}
