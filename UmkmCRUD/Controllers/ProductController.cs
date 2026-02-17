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
        ProductResponse result = await _productService.CreateProduct(req);
        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Sukses membuat produk baru",
            Data = result
        });
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(Guid id, ProductRequest req)
    {
        ProductResponse response = await _productService.UpdateProduct(id, req);
        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Sukses update produk",
            Data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProduct(id);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Product deleted successfully"
        });
    }
}
