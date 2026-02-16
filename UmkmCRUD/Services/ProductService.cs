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
    }
}