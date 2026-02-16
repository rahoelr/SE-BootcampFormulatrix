using Microsoft.AspNetCore.Http.HttpResults;
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

    public async Task<CategoryResponse> GetCategoryByID(Guid id)
    {
        Category? category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return null;
        }

        CategoryResponse result = new CategoryResponse
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description
        };

        return result;
    }

    public async Task<object> DeleteCategory(Guid id)
    {
        Category? category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return false;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CategoryResponse> UpdateCategory(Guid id, CategoryRequest request)
    {
        Category? category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return null;
        }

        if (request.CategoryName != null)
        {
            category.CategoryName = request.CategoryName;
        }

        if (request.Description != null)
        {
            category.Description = request.Description;
        }

        await _context.SaveChangesAsync();

        CategoryResponse result = new CategoryResponse
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description
        };

        return result;

    }
}