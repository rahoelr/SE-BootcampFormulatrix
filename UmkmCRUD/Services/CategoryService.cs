using UmkmCRUD.Common;
using UmkmCRUD.Repository.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ServiceResult<IEnumerable<CategoryResponse>>> GetAllCategory()
    {
        var categories = await _categoryRepository.GetAllAsync();

        var result = categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            Description = c.Description
        });

        return ServiceResult<IEnumerable<CategoryResponse>>.Success(result);
    }

    public async Task<ServiceResult<CategoryResponse>> CreateCategory(CategoryRequest dto)
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

        return ServiceResult<CategoryResponse>.Success(response);
    }

    public async Task<ServiceResult<CategoryResponse>> GetCategoryByID(Guid id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return ServiceResult<CategoryResponse>.Fail(new ServiceError(ErrorType.NotFound, "Category not found"));
        }

        CategoryResponse result = new CategoryResponse
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description
        };

        return ServiceResult<CategoryResponse>.Success(result);
    }

    public async Task<ServiceResult<bool>> DeleteCategory(Guid id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return ServiceResult<bool>.Fail(new ServiceError(ErrorType.NotFound, "Category not found"));
        }

        await _categoryRepository.DeleteAsync(category);
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<CategoryResponse>> UpdateCategory(Guid id, CategoryRequest request)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return ServiceResult<CategoryResponse>.Fail(new ServiceError(ErrorType.NotFound, "Category not found"));
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

        return ServiceResult<CategoryResponse>.Success(result);
    }
}
