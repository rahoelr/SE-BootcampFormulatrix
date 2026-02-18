using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UmkmCRUD.Common;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var result = await _authService.Register(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = result.Error?.Message
            });
        }

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Register success",
            // Data = result.Data
        });
    }


    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var result = await _authService.Login(request);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = result.Error?.Message
            });
        }

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Login success",
            Data = result.Data
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Refresh token is required"
            });
        }

        var result = await _authService.RefreshToken(request.RefreshToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = result.Error?.Message
            });
        }

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Token refreshed successfully",
            Data = result.Data
        });
    }
}
