using Microsoft.AspNetCore.Identity;

namespace OAuthServer.V2.Core.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string? Image { get; set; }
    public DateTime BirthDate { get; set; }
    public string Locale { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
