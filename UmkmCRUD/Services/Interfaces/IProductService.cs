using UmkmCRUD.Common;

namespace UmkmCRUD.Services.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResult<ProductResponse>> CreateProduct(ProductRequest request);
        Task<ServiceResult<IEnumerable<ProductResponse>>> GetProducts();
        Task<ServiceResult<ProductResponse>> GetProductById(Guid id);
        Task<ServiceResult<bool>> DeleteProduct(Guid id);
        Task<ServiceResult<ProductResponse>> UpdateProduct(Guid id, ProductRequest request);
    }
}