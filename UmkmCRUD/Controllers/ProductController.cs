using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UmkmCRUD.Common;
using UmkmCRUD.Services.Interfaces;

// [Route("api/[controller]")]
[Route("api/product")]
[ApiController]
[Authorize]
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
        var result = await _productService.GetProducts();

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<IEnumerable<ProductResponse>>
            {
                Success = false,
                Message = result.Error?.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<ProductResponse>>
        {
            Success = true,
            Message = "Sukses mengambil data produk",
            Data = result.Data
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProductById(Guid id)
    {
        var result = await _productService.GetProductById(id);

        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse<ProductResponse>
            {
                Success = false,
                Message = result.Error?.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Sukses mengambil data produk",
            Data = result.Data
        });
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(ProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProductResponse>
            {
                Success = false,
                Message = "Validation failed",
                Data = null
            });
        }

        var result = await _productService.CreateProduct(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<ProductResponse>
            {
                Success = false,
                Message = result.Error?.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Product created successfully",
            Data = result.Data
        });
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(Guid id, ProductRequest req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProductResponse>
            {
                Success = false,
                Message = "Validation failed",
                Data = null
            });
        }

        ServiceResult<ProductResponse> response = await _productService.UpdateProduct(id, req);
        if (!response.IsSuccess)
        {
            return BadRequest(new ApiResponse<ProductResponse>
            {
                Success = false,
                Message = response.Error?.Message,
                Data = null
            });
        }
        return Ok(new ApiResponse<ProductResponse>
        {
            Success = true,
            Message = "Sukses update produk",
            Data = response.Data
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteProduct(id);

        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = result.Error?.Message
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Product deleted successfully"
        });
    }
}
