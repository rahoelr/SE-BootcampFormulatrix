using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// [Route("api/[controller]")]
[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
    {
        return await _context.Products.ToListAsync();
    }

    [HttpPost("create")]
    public async Task<ActionResult<Product>> CreateProduct(ProductRequest req)

    {
        Product prod = new Product
        {
            ProductName = req.ProductName,
            Stock = req.Stock
        };
        _context.Products.Add(prod);
        await _context.SaveChangesAsync();
        return Ok(prod);
    }
}
