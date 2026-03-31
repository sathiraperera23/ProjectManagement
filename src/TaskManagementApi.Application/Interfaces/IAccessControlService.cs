using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IProjectSummaryService
    {
        Task<ProjectSummaryDto> GetProjectSummaryAsync(int projectId);
        Task SetWipLimitAsync(int projectId, SetWipLimitRequest request);
        Task<byte[]> ExportSummaryPdfAsync(int projectId);
    }

    public interface IAccessControlService
    {
        Task<bool> CanAccessAsync(int userId, AccessComponentType type, int componentId);
        Task<AccessLevel> GetAccessLevelAsync(int userId, AccessComponentType type, int componentId);

        // Rules Management
        Task<IEnumerable<AccessRuleDto>> GetProjectRulesAsync(int projectId);
        Task<AccessRuleDto> CreateRuleAsync(int projectId, CreateAccessRuleRequest request, int createdByUserId);
        Task UpdateRuleAsync(int id, CreateAccessRuleRequest request);
        Task DeleteRuleAsync(int id);

        // Requests
        Task SubmitAccessRequestAsync(int userId, CreateAccessRuleRequest request, string note);
        Task<IEnumerable<AccessRequestDto>> GetPendingRequestsAsync(int projectId);
        Task ApproveRequestAsync(int requestId, int reviewerId);
        Task RejectRequestAsync(int requestId, string reason, int reviewerId);
    }
}
