using UmkmCRUD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UmkmCRUD.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _appDbContext;

        public ProductService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<ProductResponse>> GetProducts()
        {
            var products = await _appDbContext.Products.Include(p => p.Category).ToListAsync();

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
            var categoryExists = await _appDbContext.Categories
                .AnyAsync(x => x.Id == request.CategoryId);

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

            _appDbContext.Products.Add(prod);
            await _appDbContext.SaveChangesAsync();

            await _appDbContext.Entry(prod).Reference(p => p.Category).LoadAsync();

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
            var product = await _appDbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

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
            var product = await _appDbContext.Products.FindAsync(id);
            if (product == null)
            {
                return false;
            }

            _appDbContext.Products.Remove(product);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ProductResponse> UpdateProduct(Guid id, ProductRequest request)
        {
            var product = await _appDbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

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
                var category = await _appDbContext.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    return null;
                }
                product.CategoryId = request.CategoryId;
                await _appDbContext.Entry(product).Reference(p => p.Category).LoadAsync();
            }

            await _appDbContext.SaveChangesAsync();

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