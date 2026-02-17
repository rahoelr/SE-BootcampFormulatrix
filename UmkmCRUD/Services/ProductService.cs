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
            Category? category = await _appDbContext.Categories.FindAsync(request.CategoryId);

            if (category == null)
            {
                return null;
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
    }
}