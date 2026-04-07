using TaskManagementApi.Application.DTOs.CustomerBugs;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Interfaces
{
    public interface ICustomerBugService
    {
        Task HandleInboundEmailAsync(BugInboundEmailRequest request);
        Task<IEnumerable<BugSubmissionDto>> GetSubmissionsAsync(int projectId);
        Task<IEnumerable<TicketResponse>> GetApprovalQueueAsync(int projectId);
        Task ApproveBugAsync(int ticketId, BugApprovalRequest request, int userId);
        Task RejectBugAsync(int ticketId, BugRejectionRequest request, int userId);
        Task RequestMoreInfoAsync(int ticketId, string message, int userId);

        Task<BugApprovalSlaDto?> GetSlaAsync(int projectId);
        Task UpdateSlaAsync(int projectId, UpdateBugSlaRequest request);
    }

    public interface IEmailParserService
    {
        CustomerBugSubmission Parse(string htmlBody);
    }
}
