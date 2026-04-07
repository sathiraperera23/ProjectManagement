using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Application.DTOs.Sprints;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class SprintServiceTests
    {
        private readonly Mock<IRepository<Sprint>> _sprintRepoMock;
        private readonly Mock<IRepository<Ticket>> _ticketRepoMock;
        private readonly Mock<IRepository<SprintMemberCapacity>> _capacityRepoMock;
        private readonly Mock<IRepository<SprintScopeChange>> _scopeChangeRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly SprintService _service;

        public SprintServiceTests()
        {
            _sprintRepoMock = new Mock<IRepository<Sprint>>();
            _ticketRepoMock = new Mock<IRepository<Ticket>>();
            _capacityRepoMock = new Mock<IRepository<SprintMemberCapacity>>();
            _scopeChangeRepoMock = new Mock<IRepository<SprintScopeChange>>();
            _userRepoMock = new Mock<IRepository<User>>();

            _service = new SprintService(
                _sprintRepoMock.Object,
                _ticketRepoMock.Object,
                _capacityRepoMock.Object,
                _scopeChangeRepoMock.Object,
                _userRepoMock.Object);
        }

        [Fact]
        public async Task ActivateAsync_Throws_WhenAnotherSprintIsActive()
        {
            // Arrange
            var sprintId = 1;
            var sprint = new Sprint { Id = sprintId, ProjectId = 1, SubProjectId = 1, Status = SprintStatus.Planning };
            _sprintRepoMock.Setup(r => r.GetByIdAsync(sprintId)).ReturnsAsync(sprint);

            var existingActive = new List<Sprint> { new Sprint { ProjectId = 1, SubProjectId = 1, Status = SprintStatus.Active } }.AsQueryable();
            _sprintRepoMock.SetupAsyncQueryable(existingActive);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ActivateAsync(sprintId));
        }

        [Fact]
        public async Task CloseAsync_LogsScopeChanges_ForIncompleteTickets()
        {
            // Arrange
            var sprintId = 1;
            var ticketId = 10;
            var status = new TicketStatus { IsTerminal = false };
            var ticket = new Ticket { Id = ticketId, SprintId = sprintId, Status = status };
            var sprint = new Sprint { Id = sprintId, Status = SprintStatus.Active, Tickets = new List<Ticket> { ticket } };

            _sprintRepoMock.SetupAsyncQueryable(new List<Sprint> { sprint }.AsQueryable());
            _sprintRepoMock.Setup(r => r.GetByIdAsync(sprintId)).ReturnsAsync(sprint);

            var request = new CloseSprintRequest { Disposition = SprintClosureDisposition.MoveToBacklog };

            // Act
            await _service.CloseAsync(sprintId, request, 1);

            // Assert
            Assert.Null(ticket.SprintId);
            _scopeChangeRepoMock.Verify(r => r.AddAsync(It.Is<SprintScopeChange>(c => c.ChangeType == SprintScopeChangeType.Removed)), Times.Once);
        }

        [Fact]
        public async Task CloseAsync_MovesTicketsToNextSprint()
        {
            // Arrange
            var sprintId = 1;
            var nextSprintId = 2;
            var ticketId = 10;
            var status = new TicketStatus { IsTerminal = false };
            var ticket = new Ticket { Id = ticketId, SprintId = sprintId, Status = status };
            var sprint = new Sprint { Id = sprintId, Status = SprintStatus.Active, Tickets = new List<Ticket> { ticket } };

            _sprintRepoMock.SetupAsyncQueryable(new List<Sprint> { sprint }.AsQueryable());
            _sprintRepoMock.Setup(r => r.GetByIdAsync(sprintId)).ReturnsAsync(sprint);

            var request = new CloseSprintRequest { Disposition = SprintClosureDisposition.MoveToNextSprint, NextSprintId = nextSprintId };

            // Act
            await _service.CloseAsync(sprintId, request, 1);

            // Assert
            Assert.Equal(nextSprintId, ticket.SprintId);
            _scopeChangeRepoMock.Verify(r => r.AddAsync(It.Is<SprintScopeChange>(c => c.SprintId == nextSprintId && c.ChangeType == SprintScopeChangeType.Added)), Times.Once);
        }

        [Fact]
        public async Task AddTicketToSprintAsync_LogsScopeChange_IfSprintIsActive()
        {
            // Arrange
            var sprintId = 1;
            var ticketId = 10;
            var sprint = new Sprint { Id = sprintId, Status = SprintStatus.Active };
            var ticket = new Ticket { Id = ticketId };

            _sprintRepoMock.Setup(r => r.GetByIdAsync(sprintId)).ReturnsAsync(sprint);
            _ticketRepoMock.Setup(r => r.GetByIdAsync(ticketId)).ReturnsAsync(ticket);

            // Act
            await _service.AddTicketToSprintAsync(sprintId, ticketId, "Mid-sprint add", 1);

            // Assert
            Assert.Equal(sprintId, ticket.SprintId);
            _scopeChangeRepoMock.Verify(r => r.AddAsync(It.Is<SprintScopeChange>(c => c.ChangeType == SprintScopeChangeType.Added)), Times.Once);
        }

        [Fact]
        public async Task GetVelocityAsync_CalculatesRollingAverage()
        {
            // Arrange
            var projectId = 1;
            var s1 = new Sprint { ProjectId = projectId, Status = SprintStatus.Closed, ClosedAt = DateTime.Now.AddDays(-14), Tickets = new List<Ticket>
                { new Ticket { StoryPoints = 5, Status = new TicketStatus { IsTerminal = true } } } };
            var s2 = new Sprint { ProjectId = projectId, Status = SprintStatus.Closed, ClosedAt = DateTime.Now.AddDays(-7), Tickets = new List<Ticket>
                { new Ticket { StoryPoints = 10, Status = new TicketStatus { IsTerminal = true } } } };

            var sprints = new List<Sprint> { s1, s2 }.AsQueryable();
            _sprintRepoMock.SetupAsyncQueryable(sprints);

            // Act
            var result = await _service.GetVelocityAsync(projectId);

            // Assert
            Assert.Equal(7.5, result.RollingAverageVelocity);
        }
    }
}
