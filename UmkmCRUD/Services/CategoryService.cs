using UmkmCRUD.Repository.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllCategory()
    {
        var categories = await _categoryRepository.GetAllAsync();

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

        await _categoryRepository.AddAsync(result);

        CategoryResponse response = new CategoryResponse
        {
            Id = result.Id,
            CategoryName = result.CategoryName,
            Description = result.Description
        };

        return response;
    }

    public async Task<CategoryResponse> GetCategoryByID(Guid id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
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
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(category);
        return true;
    }

    public async Task<CategoryResponse> UpdateCategory(Guid id, CategoryRequest request)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
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

        await _categoryRepository.UpdateAsync(category);

        CategoryResponse result = new CategoryResponse
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description
        };

        return result;
    }
}
