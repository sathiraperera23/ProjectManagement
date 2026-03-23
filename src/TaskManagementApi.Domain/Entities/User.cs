using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public string? MfaSecret { get; set; }
        public int FailedAttemptsCounter { get; set; }
        public DateTimeOffset? LockoutUntil { get; set; }
    }
}
