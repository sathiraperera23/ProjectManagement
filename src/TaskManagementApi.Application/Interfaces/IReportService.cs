using TaskManagementApi.Application.DTOs.Reports;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IReportService
    {
        // RTM
        Task<IEnumerable<RtmReportItemDto>> GetRtmReportAsync(int projectId, int? subProjectId = null, int? productId = null);
        Task<byte[]> ExportRtmAsync(int projectId, string format); // "pdf" or "xlsx"

        // Dependency Matrix
        Task<DependencyMatrixDto> GetDependencyMatrixAsync(int projectId, int? subProjectId = null, int? sprintId = null);
        Task<byte[]> ExportDependencyMatrixAsync(int projectId, string format);

        // Costing & P&L
        Task<CostingReportDto> GetCostingReportAsync(int projectId, int? subProjectId = null, int? productId = null);
        Task<byte[]> ExportCostingReportAsync(int projectId, string format);

        // Delays
        Task<IEnumerable<DelayReportItemDto>> GetDelayReportAsync(int projectId);
        Task<byte[]> ExportDelayReportAsync(int projectId, string format);

        // Additional
        Task<object> GetSprintReportAsync(int sprintId);
        Task<object> GetBugReportAsync(int projectId);
        Task<object> GetWorkloadReportAsync(int projectId, DateTime from, DateTime to);
        Task<object> GetTicketAgeReportAsync(int projectId);
        Task<object> GetChangeRequestLogAsync(int projectId);

        // Time Logs
        Task<TimeLogDto> LogTimeAsync(int ticketId, CreateTimeLogRequest request, int userId);
        Task<IEnumerable<TimeLogDto>> GetTicketTimeLogsAsync(int ticketId);
        Task DeleteTimeLogAsync(int logId, int userId);

        // Budget
        Task SetBudgetAsync(int projectId, SetProjectBudgetRequest request, int userId);
        Task<ProjectBudgetDto?> GetBudgetAsync(int projectId);
    }
}
