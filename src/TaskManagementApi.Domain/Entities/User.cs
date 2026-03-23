using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public string? MfaSecret { get; set; }
        public int FailedAttemptsCounter { get; set; }
        public DateTimeOffset? LockoutUntil { get; set; }

        // Keycloak / SSO Fields
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string Provider { get; set; } = null!;      // "google" or "microsoft"
        public string ProviderId { get; set; } = null!;    // Keycloak subject ID
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
