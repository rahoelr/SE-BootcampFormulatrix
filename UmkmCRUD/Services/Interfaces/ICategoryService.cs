public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllCategory();
    Task<CategoryResponse> CreateCategory(CategoryRequest dto);
    Task<CategoryResponse> GetCategoryByID(Guid id);
    Task<object> DeleteCategory(Guid id);
    Task<CategoryResponse> UpdateCategory(Guid id, CategoryRequest request);
}