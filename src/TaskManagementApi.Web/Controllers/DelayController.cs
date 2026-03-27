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
    }
}
