namespace UmkmCRUD.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProduct(ProductRequest request);
        Task<IEnumerable<ProductResponse>> GetProducts();
        Task<ProductResponse> GetProductById(Guid id);
        Task<object> DeleteProduct(Guid id);
        Task<ProductResponse> UpdateProduct(Guid id, ProductRequest request);
    }
}