public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllCategory();
    Task<CategoryResponse> CreateCategory(CategoryRequest dto);
}