using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Application.DTOs.Reports;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class ReportServiceTests
    {
        private readonly Mock<IRepository<BacklogItem>> _backlogRepo;
        private readonly Mock<IRepository<Ticket>> _ticketRepo;
        private readonly Mock<IRepository<TicketLink>> _ticketLinkRepo;
        private readonly Mock<IRepository<TimeLog>> _timeLogRepo;
        private readonly Mock<IRepository<UserRate>> _userRateRepo;
        private readonly Mock<IRepository<ProjectBudget>> _budgetRepo;
        private readonly Mock<IRepository<DelayRecord>> _delayRepo;
        private readonly Mock<IRepository<Sprint>> _sprintRepo;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _backlogRepo = new Mock<IRepository<BacklogItem>>();
            _ticketRepo = new Mock<IRepository<Ticket>>();
            _ticketLinkRepo = new Mock<IRepository<TicketLink>>();
            _timeLogRepo = new Mock<IRepository<TimeLog>>();
            _userRateRepo = new Mock<IRepository<UserRate>>();
            _budgetRepo = new Mock<IRepository<ProjectBudget>>();
            _delayRepo = new Mock<IRepository<DelayRecord>>();
            _sprintRepo = new Mock<IRepository<Sprint>>();

            _service = new ReportService(
                _backlogRepo.Object, _ticketRepo.Object, _ticketLinkRepo.Object,
                _timeLogRepo.Object, _userRateRepo.Object, _budgetRepo.Object,
                _delayRepo.Object, _sprintRepo.Object);
        }

        [Fact]
        public async Task GetCostingReport_CalculatesCostPerTicketCorrectly()
        {
            // Arrange
            var projectId = 1;
            var userId = 10;
            var ticketId = 100;
            var logDate = DateTime.UtcNow;

            var budget = new ProjectBudget { ProjectId = projectId, ContractValue = 1000, BudgetAmount = 800 };
            _budgetRepo.SetupAsyncQueryable(new List<ProjectBudget> { budget }.AsQueryable());

            var timeLog = new TimeLog { TicketId = ticketId, UserId = userId, HoursLogged = 5, LoggedAt = logDate };
            var ticket = new Ticket { Id = ticketId, ProjectId = projectId };
            timeLog.Ticket = ticket;
            _timeLogRepo.SetupAsyncQueryable(new List<TimeLog> { timeLog }.AsQueryable());

            var rate = new UserRate { UserId = userId, HourlyRate = 100, EffectiveFrom = logDate.AddDays(-1) };
            _userRateRepo.SetupAsyncQueryable(new List<UserRate> { rate }.AsQueryable());

            _ticketRepo.SetupAsyncQueryable(new List<Ticket> { ticket }.AsQueryable());

            // Act
            var result = await _service.GetCostingReportAsync(projectId);

            // Assert
            Assert.Equal(500, result.TotalCost); // 5 hours * 100/hr
            Assert.Equal(500, result.ProfitLoss); // 1000 - 500
            Assert.Equal(50, result.MarginPercentage); // 500 / 1000 * 100
        }

        [Fact]
        public async Task GetRtmReport_DetectsCoverageGap()
        {
            // Arrange
            var projectId = 1;
            var itemWithTicket = new BacklogItem { Id = 1, ProjectId = projectId, TicketLinks = new List<BacklogItemTicketLink> { new BacklogItemTicketLink { Ticket = new Ticket { TicketNumber = "T1" } } } };
            var itemWithoutTicket = new BacklogItem { Id = 2, ProjectId = projectId, TicketLinks = new List<BacklogItemTicketLink>() };

            _backlogRepo.SetupAsyncQueryable(new List<BacklogItem> { itemWithTicket, itemWithoutTicket }.AsQueryable());

            // Act
            var result = await _service.GetRtmReportAsync(projectId);

            // Assert
            Assert.False(result.First(r => r.RequirementId == "BRD-1").IsCoverageGap);
            Assert.True(result.First(r => r.RequirementId == "BRD-2").IsCoverageGap);
        }
    }
}
