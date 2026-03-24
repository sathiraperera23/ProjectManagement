using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Web.Authorization
{
    public class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public PermissionAuthorizationHandler(
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Extract SSO ProviderId (Keycloak 'sub') from JWT claims
            var providerId = context.User.FindFirst("sub")?.Value;
            if (providerId == null)
            {
                providerId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            if (providerId == null)
            {
                context.Fail();
                return;
            }

            // Look up local user by ProviderId
            // In a real high-traffic app, we might cache this mapping
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.ProviderId == providerId);
            if (user == null)
            {
                context.Fail();
                return;
            }

            var userId = user.Id;

            // Extract projectId from route values
            var httpContext = _httpContextAccessor.HttpContext;
            var projectIdStr = httpContext?.Request.RouteValues["projectId"]?.ToString();

            if (!int.TryParse(projectIdStr, out var projectId))
            {
                projectId = 0;
            }

            var hasPermission = await _permissionService
                .HasPermissionAsync(userId, projectId, requirement.Permission);

            if (hasPermission)
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}
