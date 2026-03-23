namespace TaskManagementApi.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, int projectId, string permission);
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, int projectId);
    }
}
