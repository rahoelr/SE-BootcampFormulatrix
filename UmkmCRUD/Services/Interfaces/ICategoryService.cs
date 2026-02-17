using UmkmCRUD.Common;

public interface ICategoryService
{
    Task<ServiceResult<IEnumerable<CategoryResponse>>> GetAllCategory();
    Task<ServiceResult<CategoryResponse>> CreateCategory(CategoryRequest dto);
    Task<ServiceResult<CategoryResponse>> GetCategoryByID(Guid id);
    Task<ServiceResult<bool>> DeleteCategory(Guid id);
    Task<ServiceResult<CategoryResponse>> UpdateCategory(Guid id, CategoryRequest request);
}