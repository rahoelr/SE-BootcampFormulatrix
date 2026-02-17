using AutoMapper;
using UmkmCRUD.Common;
using UmkmCRUD.Repository.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IEnumerable<CategoryResponse>>> GetAllCategory()
    {
        var categories = await _categoryRepository.GetAllAsync();
        
        var result = _mapper.Map<IEnumerable<CategoryResponse>>(categories);

        return ServiceResult<IEnumerable<CategoryResponse>>.Success(result);
    }

    public async Task<ServiceResult<CategoryResponse>> CreateCategory(CategoryRequest dto)
    {
        var entity = _mapper.Map<Category>(dto);

        await _categoryRepository.AddAsync(entity);

        CategoryResponse response = _mapper.Map<CategoryResponse>(entity);

        return ServiceResult<CategoryResponse>.Success(response);
    }

    public async Task<ServiceResult<CategoryResponse>> GetCategoryByID(Guid id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return ServiceResult<CategoryResponse>.Fail(new ServiceError(ErrorType.NotFound, "Category not found"));
        }

        var result = _mapper.Map<CategoryResponse>(category);

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

        var result = _mapper.Map<CategoryResponse>(category);

        return ServiceResult<CategoryResponse>.Success(result);
    }
}
