using Microsoft.AspNetCore.Identity;

namespace Task4UserAdmin.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public DateTimeOffset RegisteredAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAtUtc { get; set; }
}
