using UmkmCRUD.Services.Interfaces;
using UmkmCRUD.Repository.Interfaces;

namespace UmkmCRUD.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<ProductResponse>> GetProducts()
        {
            var products = await _productRepository.GetAllAsync();

            var response = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                ProductName = p.ProductName,
                Stock = p.Stock,
                CategoryName = p.Category?.CategoryName
            }).ToList();

            return response;
        }

        public async Task<ProductResponse> CreateProduct(ProductRequest request)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

            if (!categoryExists)
            {
                throw new Exception("Category not found");
            }

            Product prod = new Product
            {
                ProductName = request.ProductName,
                Stock = request.Stock ?? 0,
                CategoryId = request.CategoryId
            };

            await _productRepository.AddAsync(prod);
            await _productRepository.LoadCategoryAsync(prod);

            var response = new ProductResponse
            {
                Id = prod.Id,
                ProductName = prod.ProductName,
                Stock = prod.Stock,
                CategoryId = prod.CategoryId,
                CategoryName = prod.Category?.CategoryName
            };

            return response;
        }

        public async Task<ProductResponse> GetProductById(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return null;
            }

            var response = new ProductResponse
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName
            };

            return response;
        }

        public async Task<object> DeleteProduct(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            await _productRepository.DeleteAsync(product);

            return true;
        }

        public async Task<ProductResponse> UpdateProduct(Guid id, ProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return null;
            }

            if (request.ProductName != null)
            {
                product.ProductName = request.ProductName;
            }

            if (request.Stock.HasValue)
            {
                product.Stock = request.Stock.Value;
            }

            if (request.CategoryId != Guid.Empty && request.CategoryId != product.CategoryId)
            {
                var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
                if (!categoryExists)
                {
                    return null;
                }
                product.CategoryId = request.CategoryId;
                await _productRepository.UpdateAsync(product);
                await _productRepository.LoadCategoryAsync(product);
            }
            else
            {
                await _productRepository.UpdateAsync(product);
            }

            var response = new ProductResponse
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName
            };
            return response;
        }
    }
}
