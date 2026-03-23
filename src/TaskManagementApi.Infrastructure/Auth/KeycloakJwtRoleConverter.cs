using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace TaskManagementApi.Infrastructure.Auth
{
    public class KeycloakJwtRoleConverter : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity!;
            var realmAccess = principal.FindFirst("realm_access")?.Value;
            if (realmAccess != null)
            {
                var parsed = JsonDocument.Parse(realmAccess);
                if (parsed.RootElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (roleName != null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                        }
                    }
                }
            }
            return Task.FromResult(principal);
        }
    }
}
