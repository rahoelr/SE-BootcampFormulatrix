using Microsoft.EntityFrameworkCore;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllCategory()
    {
        var categories = await _context.Categories.ToListAsync();

        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            Description = c.Description
        });
    }
    public async Task<CategoryResponse> CreateCategory(CategoryRequest dto)
    {
        Category result = new Category
        {
            CategoryName = dto.CategoryName,
            Description = dto.Description
        };
        _context.Categories.Add(result);
        await _context.SaveChangesAsync();

        CategoryResponse response = new CategoryResponse
        {
            CategoryName = result.CategoryName,
            Description = result.Description
        };

        return response;
    }
}