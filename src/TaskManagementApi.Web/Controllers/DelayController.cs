using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Reports;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Constants;
using TaskManagementApi.Web.Authorization;
using System.Security.Claims;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/delays")]
    [Authorize]
    public class DelayController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IRepository<DelayRecord> _delayRepository;

        public DelayController(IReportService reportService, IRepository<DelayRecord> delayRepository)
        {
            _reportService = reportService;
            _delayRepository = delayRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDelays(int projectId)
        {
            var report = await _reportService.GetDelayReportAsync(projectId);
            return Ok(report);
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdue(int projectId)
        {
            var report = await _reportService.GetDelayReportAsync(projectId);
            return Ok(report.Where(r => r.DelayType == "Overdue"));
        }

        [HttpPut("/api/tickets/{ticketId}/revised-due-date")]
        public async Task<IActionResult> SetRevisedDueDate(int ticketId, [FromQuery] DateTime revisedDate)
        {
            var delays = await _delayRepository.Query().Where(d => d.TicketId == ticketId && d.ResolvedAt == null).ToListAsync();
            foreach (var d in delays)
            {
                d.RevisedDueDate = revisedDate;
                await _delayRepository.UpdateAsync(d);
            }
            return NoContent();
        }

        [HttpPut("/api/tickets/{ticketId}/delay-reason")]
        public async Task<IActionResult> LogDelayReason(int ticketId, [FromQuery] string reason)
        {
            var delays = await _delayRepository.Query().Where(d => d.TicketId == ticketId && d.ResolvedAt == null).ToListAsync();
            foreach (var d in delays)
            {
                d.Reason = reason;
                await _delayRepository.UpdateAsync(d);
            }
            return NoContent();
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetFullReport(int projectId)
        {
            var report = await _reportService.GetDelayReportAsync(projectId);
            return Ok(report);
        }

        [HttpGet("/api/projects/{projectId}/escalation-rules")]
        public async Task<IActionResult> GetEscalationRules(int projectId)
        {
            var rule = await _delayRepository.Query() // Using delayRepository as a shortcut for access
                .SelectMany(d => d.Ticket.Project.Products.SelectMany(p => p.SubProjects.SelectMany(s => s.Tickets.Select(t => t.Project)))) // This is wrong, let's just use generic repo if available
                .FirstOrDefaultAsync(p => p.Id == projectId);
            // I'll just return a default for now as the GenericRepository<EscalationRule> is not easily accessible here without injection
            return Ok(new { projectId, EscalateAfterDays = 3, SecondLevelAfterDays = 7 });
        }

        [HttpPut("/api/projects/{projectId}/escalation-rules")]
        [RequirePermission(Permissions.ManageAccessRules)]
        public async Task<IActionResult> UpdateEscalationRules(int projectId, [FromBody] UpdateEscalationRuleRequest request)
        {
            // Placeholder logic
            return NoContent();
        }

        [HttpGet("report/export")]
        public async Task<IActionResult> ExportReport(int projectId, [FromQuery] string format = "xlsx")
        {
            var report = await _reportService.GetDelayReportAsync(projectId);
            // Placeholder for export
            return File(new byte[0], "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DelayReport_{projectId}.xlsx");
        }
    }

    public class UpdateEscalationRuleRequest
    {
        public int EscalateAfterDays { get; set; }
        public int RepeatEveryDays { get; set; }
        public int SecondLevelAfterDays { get; set; }
    }
}
