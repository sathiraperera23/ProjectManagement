using Microsoft.AspNetCore.Authorization;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Authorization
{
    public class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionAuthorizationHandler(
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Extract userId from JWT claims (Keycloak uses 'sub' or custom claim)
            var userIdStr = context.User.FindFirst("sub")?.Value;
            if (userIdStr == null)
            {
                // Fallback to NameIdentifier if sub is not present
                userIdStr = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            if (userIdStr == null)
            {
                context.Fail();
                return;
            }

            // Extract projectId from route values
            var httpContext = _httpContextAccessor.HttpContext;
            var projectIdStr = httpContext?.Request.RouteValues["projectId"]?.ToString();

            // If projectId is not in route, maybe it's in query or not required for this permission
            // For now, following requirements: extract from route values.
            if (!int.TryParse(projectIdStr, out var projectId))
            {
                // If permission doesn't require a project context (e.g., ManageUsers),
                // we might need a different handler or logic.
                // But for this scaffold, we'll assume projectId is required or use 0.
                projectId = 0;
            }

            if (!int.TryParse(userIdStr, out var userId))
            {
                // In some systems, userId is a Guid from SSO.
                // But our User entity uses 'int'.
                // We assume the mapped User.Id is what's in the claim.
                context.Fail();
                return;
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
