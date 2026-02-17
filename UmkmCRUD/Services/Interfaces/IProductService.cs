namespace UmkmCRUD.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProduct(ProductRequest request);
        Task<IEnumerable<ProductResponse>> GetProducts();
        Task<ProductResponse> GetProductById(Guid id);
    }
}