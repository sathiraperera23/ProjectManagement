using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskManagementApi.Application.DTOs.Projects;

namespace TaskManagementApi.Tests
{
    public class ProjectSummaryServiceTests
    {
        private readonly Mock<IRepository<Project>> _projectRepo;
        private readonly Mock<IRepository<Ticket>> _ticketRepo;
        private readonly Mock<IRepository<Milestone>> _milestoneRepo;
        private readonly Mock<IRepository<Sprint>> _sprintRepo;
        private readonly Mock<IRepository<Team>> _teamRepo;
        private readonly Mock<IRepository<DailyUpdate>> _dailyUpdateRepo;
        private readonly Mock<IRepository<ProjectWipLimit>> _wipLimitRepo;
        private readonly Mock<IRepository<ProjectBudget>> _budgetRepo;
        private readonly Mock<IRepository<TimeLog>> _timeLogRepo;
        private readonly Mock<IRepository<DelayRecord>> _delayRepo;
        private readonly ProjectSummaryService _service;

        public ProjectSummaryServiceTests()
        {
            _projectRepo = new Mock<IRepository<Project>>();
            _ticketRepo = new Mock<IRepository<Ticket>>();
            _milestoneRepo = new Mock<IRepository<Milestone>>();
            _sprintRepo = new Mock<IRepository<Sprint>>();
            _teamRepo = new Mock<IRepository<Team>>();
            _dailyUpdateRepo = new Mock<IRepository<DailyUpdate>>();
            _wipLimitRepo = new Mock<IRepository<ProjectWipLimit>>();
            _budgetRepo = new Mock<IRepository<ProjectBudget>>();
            _timeLogRepo = new Mock<IRepository<TimeLog>>();
            _delayRepo = new Mock<IRepository<DelayRecord>>();

            _service = new ProjectSummaryService(
                _projectRepo.Object, _ticketRepo.Object, _milestoneRepo.Object,
                _sprintRepo.Object, _teamRepo.Object, _dailyUpdateRepo.Object,
                _wipLimitRepo.Object, _budgetRepo.Object, _timeLogRepo.Object,
                _delayRepo.Object);
        }

        [Fact]
        public async Task GetProjectSummary_CalculatesWipLevel_Correctly()
        {
            // Arrange
            var projectId = 1;
            var project = new Project { Id = projectId, Name = "Test Project" };
            _projectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // 8 tickets in WIP-related statuses
            var tickets = new List<Ticket>();
            for (int i = 0; i < 8; i++)
                tickets.Add(new Ticket { Id = i+1, ProjectId = projectId, Status = new TicketStatus { Name = "WIP" } });

            _ticketRepo.SetupAsyncQueryable(tickets.AsQueryable());

            // Limit is 10
            var limit = new ProjectWipLimit { ProjectId = projectId, MaxWip = 10 };
            _wipLimitRepo.SetupAsyncQueryable(new List<ProjectWipLimit> { limit }.AsQueryable());

            // Other mocks to avoid nulls
            _milestoneRepo.SetupAsyncQueryable(new List<Milestone>().AsQueryable());
            _sprintRepo.SetupAsyncQueryable(new List<Sprint>().AsQueryable());
            _dailyUpdateRepo.SetupAsyncQueryable(new List<DailyUpdate>().AsQueryable());
            _delayRepo.SetupAsyncQueryable(new List<DelayRecord>().AsQueryable());
            _budgetRepo.SetupAsyncQueryable(new List<ProjectBudget>().AsQueryable());

            // Act
            var result = await _service.GetProjectSummaryAsync(projectId);

            // Assert
            Assert.Equal(8, result.WipStatus.Count);
            Assert.Equal("Amber", result.WipStatus.IndicatorLevel); // 80% is Amber
        }

        [Fact]
        public async Task GetProjectSummary_CalculatesWipLevelRed_WhenExceeded()
        {
            // Arrange
            var projectId = 1;
            var project = new Project { Id = projectId };
            _projectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            var tickets = new List<Ticket>();
            for (int i = 0; i < 11; i++)
                tickets.Add(new Ticket { Id = i+1, ProjectId = projectId, Status = new TicketStatus { Name = "WIP" } });
            _ticketRepo.SetupAsyncQueryable(tickets.AsQueryable());

            var limit = new ProjectWipLimit { ProjectId = projectId, MaxWip = 10 };
            _wipLimitRepo.SetupAsyncQueryable(new List<ProjectWipLimit> { limit }.AsQueryable());

            _milestoneRepo.SetupAsyncQueryable(new List<Milestone>().AsQueryable());
            _sprintRepo.SetupAsyncQueryable(new List<Sprint>().AsQueryable());
            _dailyUpdateRepo.SetupAsyncQueryable(new List<DailyUpdate>().AsQueryable());
            _delayRepo.SetupAsyncQueryable(new List<DelayRecord>().AsQueryable());
            _budgetRepo.SetupAsyncQueryable(new List<ProjectBudget>().AsQueryable());

            // Act
            var result = await _service.GetProjectSummaryAsync(projectId);

            // Assert
            Assert.Equal("Red", result.WipStatus.IndicatorLevel);
        }
    }
}
