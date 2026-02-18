using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
