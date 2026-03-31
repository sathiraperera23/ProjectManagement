namespace TaskManagementApi.Application.Interfaces
{
    public interface IBugReportTemplateService
    {
        Task<string> GetTemplateAsync(int projectId);
        Task UpdateCustomTextAsync(int projectId, string customText, int updatedByUserId);
    }
}
