using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Models
{
    public class User : IdentityUser
    {
        public string? MfaSecret { get; set; }
        public int FailedAttemptsCounter { get; set; }
        public DateTimeOffset? LockoutUntil { get; set; }
    }
}
