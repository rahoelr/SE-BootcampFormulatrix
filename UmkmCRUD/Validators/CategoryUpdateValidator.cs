using FluentValidation;

public class CategoryUpdateValidator : AbstractValidator<CategoryRequest>
{
    public CategoryUpdateValidator()
    {
        RuleFor(c => c.CategoryName)
            .NotEmpty().WithMessage("Nama kategori wajib di isi")
            .MinimumLength(3).WithMessage("Nama minimal 3 karakter")
            .When(c => c.CategoryName != null);
        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Deskripsi wajib diisi")
            .MinimumLength(10).WithMessage("Deskripsi minimal 10 karakter")
            .When(c => c.Description != null);
    }
}   