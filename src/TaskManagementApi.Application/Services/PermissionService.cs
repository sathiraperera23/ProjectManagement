using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRoleService _roleService;

        public PermissionService(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<bool> HasPermissionAsync(int userId, int projectId, string permission)
        {
            var permissions = await _roleService.GetEffectivePermissionsAsync(userId, projectId);
            return permissions.Contains(permission);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, int projectId)
        {
            return await _roleService.GetEffectivePermissionsAsync(userId, projectId);
        }
    }
}
