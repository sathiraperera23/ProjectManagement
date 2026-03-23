using Microsoft.AspNetCore.Authorization;

namespace TaskManagementApi.Web.Authorization
{
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
            : base(policy: $"Permission:{permission}")
        {
            Permission = permission;
        }
        public string Permission { get; }
    }
}
