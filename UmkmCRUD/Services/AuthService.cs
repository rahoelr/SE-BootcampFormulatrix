using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UmkmCRUD.Common;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IMapper mapper)
    {
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
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

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return ServiceResult<AuthResponse>.Fail(
                new ServiceError(ErrorType.Validation, result.Errors.First().Description));
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);
        
        var roles = await _userManager.GetRolesAsync(user);

        var token = GenerateJwtToken(user, roles);

        var response = new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Roles = roles.ToList()
        };

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

        var token = GenerateJwtToken(user, roles);

        var response = new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Roles = roles.ToList()
        };

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
            expires: DateTime.Now.AddMinutes(
                Convert.ToDouble(jwtSettings["DurationInMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
