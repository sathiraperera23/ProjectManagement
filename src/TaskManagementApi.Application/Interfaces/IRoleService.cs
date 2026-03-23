using TaskManagementApi.Application.DTOs.Roles;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, string createdByUserId);
        Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleRequest request, string updatedByUserId);
        Task DeleteRoleAsync(int id, string deletedByUserId);
        Task UpdateRolePermissionsAsync(int id, UpdateRolePermissionsRequest request, string updatedByUserId);
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<IEnumerable<string>> GetEffectivePermissionsAsync(int userId, int projectId);
        Task AssignRoleToUserAsync(AssignRoleRequest request, string assignedByUserId);
        Task RemoveRoleFromUserAsync(int userId, int projectId, string removedByUserId);
    }
}
