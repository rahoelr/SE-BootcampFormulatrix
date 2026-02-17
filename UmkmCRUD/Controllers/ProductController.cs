using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UmkmCRUD.Services.Interfaces;

// [Route("api/[controller]")]
[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
    private IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> GetProduct()
    {
        IEnumerable<ProductResponse> result = await _productService.GetProducts();
        return Ok(new ApiResponse<IEnumerable<ProductResponse>>
        {
            Success = true,
            Message = "Sukses mengambil data produk",
            Data = result
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProductById(Guid id)
    {
        ProductResponse response = await _productService.GetProductById(id);

        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Sukses mengambil data produk",
            Data = response
        });
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> CreateProduct(ProductRequest req)
    {
        ProductResponse result  = await _productService.CreateProduct(req);
        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Sukses membuat produk baru",
            Data = result
        });
    }

    // [HttpPatch("{id}")]
    // public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(Guid id, ProductRequest req)
    // {
    //     var product = await _context.Products.FindAsync(id);

    //     if (product == null)
    //     {
    //         return NotFound(new ApiResponse<ProductResponse>
    //         {
    //             Success = false,
    //             Message = "Produk tidak ditemukan",
    //             Data = null
    //         });
    //     }

    //     if (req.ProductName != null)
    //     {
    //         product.ProductName = req.ProductName;
    //     }

    //     // Jika CategoryId berubah, validasi dulu
    //     if (req.CategoryId != Guid.Empty && req.CategoryId != product.CategoryId)
    //     {
    //         var category = await _context.Categories.FindAsync(req.CategoryId);
    //         if (category == null)
    //         {
    //             return BadRequest(new ApiResponse<ProductResponse>
    //             {
    //                 Success = false,
    //                 Message = "Category ID baru tidak ditemukan",
    //                 Data = null
    //             });
    //         }
    //         product.CategoryId = req.CategoryId;
    //     }

    //     await _context.SaveChangesAsync();

    //     await _context.Entry(product).Reference(p => p.Category).LoadAsync();

    //     var response = new ProductResponse
    //     {
    //         Id = product.Id,
    //         ProductName = product.ProductName,
    //         Stock = product.Stock,
    //         CategoryId = product.CategoryId,
    //         CategoryName = product.Category?.CategoryName
    //     };

    //     return Ok(new ApiResponse<ProductResponse>
    //     {
    //         Success = true,
    //         Message = "Sukses update produk",
    //         Data = response
    //     });
    // }

    // [HttpDelete("{id}")]
    // public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    // {
    //     var product = await _context.Products.FindAsync(id);
    //     if (product == null)
    //     {
    //         return NotFound(new ApiResponse<object>
    //         {
    //             Success = false,
    //             Message = "Produk tidak ditemukan",
    //             Data = null
    //         });
    //     }

    //     _context.Products.Remove(product);
    //     await _context.SaveChangesAsync();

    //     return Ok(new ApiResponse<object>
    //     {
    //         Success = true,
    //         Message = "Produk berhasil dihapus",
    //         Data = null
    //     });
    // }
}
