using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UmkmCRUD.Common;

// [Route("api/[controller]")]
[Route("api/category")]
[ApiController]
public class CategoryController : ControllerBase
{
    public ICategoryService _categoryService;
    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetCategory()
    {
        var result = await _categoryService.GetAllCategory();

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<IEnumerable<CategoryResponse>>
            {
                Success = false,
                Message = result.Error?.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<IEnumerable<CategoryResponse>>
        {
            Success = true,
            Message = "Sukses mengambil data kategori",
            Data = result.Data
        });
    }


    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> CreateCategory(CategoryRequest req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = ModelState
            });
        }

        ServiceResult<CategoryResponse> response = await _categoryService.CreateCategory(req);

        if (!response.IsSuccess)
        {
            return BadRequest(new ApiResponse<CategoryResponse>
            {
                Success = false,
                Message = response.Error?.Message,
                Data = null
            });
        }

        return Ok(new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "Sukses membuat kategori baru nih",
            Data = response.Data
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategory(id);

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
            Message = "Category deleted successfully"
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetCategoryByID(Guid id)
    {
        var result = await _categoryService.GetCategoryByID(id);

        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse<CategoryResponse>
            {
                Success = false,
                Message = result.Error?.Message
            });
        }

        return Ok(new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "Sukses mengambil data kategori",
            Data = result.Data
        });
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> UpdateCategory(Guid id, CategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = ModelState
            });
        }

        var result = await _categoryService.UpdateCategory(id, request);

        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = result.Error?.Message
            });
        }

        return Ok(new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "Sukses update kategori",
            Data = result.Data
        });
    }

}
