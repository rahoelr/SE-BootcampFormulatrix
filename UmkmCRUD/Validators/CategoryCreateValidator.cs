using FluentValidation;

public class CategoryCreateValidator : AbstractValidator<CategoryRequest>
{
    public CategoryCreateValidator()
    {
        RuleFor(c => c.CategoryName)
            .NotEmpty().WithMessage("Nama kategori wajib di isi")
            .MinimumLength(3).WithMessage("Nama minimal 3 karakter");
        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Deskripsi wajib diisi")
            .MinimumLength(10).WithMessage("Deskripsi minimal 10 karakter");
    }
}