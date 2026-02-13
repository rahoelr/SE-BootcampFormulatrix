using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// [Route("api/[controller]")]
[Route("api/category")]
[ApiController]
public class CategoryController : ControllerBase
{
    public ICategoryService _categoryService;
    public CategoryController (ICategoryService categoryService){
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetCategory()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        IEnumerable<CategoryResponse> response = await _categoryService.GetAllCategory();


        return Ok(new ApiResponse<IEnumerable<CategoryResponse>>
        {
            Success = true,
            Message = "Sukses mengambil data kategori",
            Data = response
        });
    }


    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> CreateCategory(CategoryRequest req)

    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        CategoryResponse response = await _categoryService.CreateCategory(req);

        return Ok(new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "Sukses membuat kategori baru nih",
            Data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
    {
        await _categoryService.DeleteCategory(id);
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

        if (result == null)
        {
            return NotFound(new ApiResponse<CategoryResponse>
            {
                Success = false,
                Message = "Data tidak ditemukan",
                Data = result
            });
        }

        return Ok(new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "sukses mengambil data",
            Data = result
        });
    }

    // [HttpPatch("{id}")]
    // public async Task<ActionResult<ApiResponse<CategoryResponse>>> UpdateCategory(Guid id, CategoryRequest request)
    // {
    //     Category? category = await _context.Categories.FindAsync(id);

    //     if (category == null)
    //     {
    //         return NotFound(new ApiResponse<CategoryResponse>
    //         {
    //             Success = false,
    //             Message = "tidak ditemukan datanya",
    //             Data = null
    //         });
    //     }

    //     if (request.CategoryName != null)
    //     {
    //         category.CategoryName = request.CategoryName;
    //     }

    //     await _context.SaveChangesAsync();

    //     CategoryResponse result = new CategoryResponse
    //     {
    //         Id = category.Id,
    //         CategoryName = category.CategoryName,
    //         Description = category.Description
    //     };

    //     return Ok(new ApiResponse<CategoryResponse>
    //     {
    //         Success = true,
    //         Message = "sukses update kategori",
    //         Data = result,
    //     });
    // }

}
