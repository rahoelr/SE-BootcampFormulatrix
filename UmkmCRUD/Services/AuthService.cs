using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UmkmCRUD.Common;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IMapper mapper,
        AppDbContext context)
    {
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<ServiceResult<AuthResponse>> Register(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.Validation, "Email already registered"));
        }

        var user = _mapper.Map<ApplicationUser>(request);

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.Validation, createResult.Errors.First().Description));
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        refreshToken.ApplicationUserId = user.Id;

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<AuthResponse>(user);
        response.Token = accessToken;
        response.RefreshToken = refreshToken.Token;
        response.Roles = roles.ToList();

        return ServiceResult<AuthResponse>.Success(response);
    }

    public async Task<ServiceResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.NotFound, "User not found"));
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!validPassword)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.Validation, "Invalid password"));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        refreshToken.ApplicationUserId = user.Id;

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<AuthResponse>(user);
        response.Token = accessToken;
        response.RefreshToken = refreshToken.Token;
        response.Roles = roles.ToList();

        return ServiceResult<AuthResponse>.Success(response);
    }

    public async Task<ServiceResult<AuthResponse>> RefreshToken(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(t => t.ApplicationUser)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

        if (tokenEntity == null)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.Validation, "Invalid refresh token"));
        }

        if (tokenEntity.Expires < DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.Validation, "Refresh token expired"));
        }

        tokenEntity.IsRevoked = true;

        var user = tokenEntity.ApplicationUser!;
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = GenerateRefreshToken();
        newRefreshToken.ApplicationUserId = user.Id;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<AuthResponse>(user);
        response.Token = newAccessToken;
        response.RefreshToken = newRefreshToken.Token;
        response.Roles = roles.ToList();

        return ServiceResult<AuthResponse>.Success(response);
    }


    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(jwtSettings["AccessTokenExpirationMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(
                Convert.ToDouble(jwtSettings["RefreshTokenExpirationDays"])),
            IsRevoked = false
        };
    }

}
