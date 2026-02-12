using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// [Route("api/[controller]")]
[Route("api/category")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetCategory()
    {
        var categories = await _context.Categories.ToListAsync();

        var response = categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            CategoryName = c.CategoryName
        }).ToList();

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
        var category = new Category
        {
            CategoryName = req.CategoryName
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryResponse
        {
            Id = category.Id,
            CategoryName = category.CategoryName
        };

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
        Category? category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Category not found",
                Data = null
            });
        }

        _context.Categories.Remove(category);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Category deleted successfully"
        });
    }
}
