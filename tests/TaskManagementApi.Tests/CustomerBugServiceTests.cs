using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Application.DTOs.CustomerBugs;
using TaskManagementApi.Application.DTOs.Tickets;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaskManagementApi.Tests
{
    public class CustomerBugServiceTests
    {
        private readonly Mock<IRepository<CustomerBugSubmission>> _submissionRepo;
        private readonly Mock<IRepository<Ticket>> _ticketRepo;
        private readonly Mock<IRepository<Project>> _projectRepo;
        private readonly Mock<IRepository<BugApprovalSla>> _slaRepo;
        private readonly Mock<IEmailParserService> _parserMock;
        private readonly Mock<ITicketService> _ticketServiceMock;
        private readonly Mock<INotificationService> _notifMock;
        private readonly Mock<IEmailService> _emailMock;
        private readonly Mock<IBugReportTemplateService> _templateMock;
        private readonly CustomerBugService _service;

        public CustomerBugServiceTests()
        {
            _submissionRepo = new Mock<IRepository<CustomerBugSubmission>>();
            _ticketRepo = new Mock<IRepository<Ticket>>();
            _projectRepo = new Mock<IRepository<Project>>();
            _slaRepo = new Mock<IRepository<BugApprovalSla>>();
            _parserMock = new Mock<IEmailParserService>();
            _ticketServiceMock = new Mock<ITicketService>();
            _notifMock = new Mock<INotificationService>();
            _emailMock = new Mock<IEmailService>();
            _templateMock = new Mock<IBugReportTemplateService>();

            _service = new CustomerBugService(
                _submissionRepo.Object, _ticketRepo.Object, _projectRepo.Object,
                _slaRepo.Object, _parserMock.Object, _ticketServiceMock.Object,
                _notifMock.Object, _emailMock.Object, _templateMock.Object,
                new Mock<IConfiguration>().Object,
                new Mock<ILogger<CustomerBugService>>().Object);
        }

        [Fact]
        public async Task HandleInboundEmail_FlagsDuplicate_WhenTitleMatches()
        {
            // Arrange
            var projectId = 1;
            var intakeEmail = "bugs@project.com";
            var request = new BugInboundEmailRequest { To = intakeEmail, From = "cust@example.com", Html = "..." };

            var project = new Project { Id = projectId, IntakeEmailAddress = intakeEmail };
            _projectRepo.SetupAsyncQueryable(new List<Project> { project }.AsQueryable());

            var submission = new CustomerBugSubmission { ParsedTitle = "Existing Bug" };
            _parserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(submission);

            _ticketRepo.SetupAsyncQueryable(new List<Ticket> { new Ticket { ProjectId = projectId, Title = "Existing Bug" } }.AsQueryable());

            // Act
            await _service.HandleInboundEmailAsync(request);

            // Assert
            Assert.Equal(BugParseStatus.Duplicate, submission.ParseStatus);
            _submissionRepo.Verify(r => r.AddAsync(submission), Times.Once);
            _ticketServiceMock.Verify(s => s.CreateTicketAsync(It.IsAny<CreateTicketRequest>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task HandleInboundEmail_FlagsInvalid_WhenFieldsMissing()
        {
            // Arrange
            var project = new Project { Id = 1, IntakeEmailAddress = "bugs@p.com" };
            _projectRepo.SetupAsyncQueryable(new List<Project> { project }.AsQueryable());
            _ticketRepo.SetupAsyncQueryable(new List<Ticket>().AsQueryable());

            var submission = new CustomerBugSubmission { ParsedTitle = "" }; // Title missing
            _parserMock.Setup(p => p.Parse(It.IsAny<string>())).Returns(submission);

            // Act
            await _service.HandleInboundEmailAsync(new BugInboundEmailRequest { To = "bugs@p.com", From = "c@e.com", Html = "..." });

            // Assert
            Assert.Equal(BugParseStatus.InvalidFormat, submission.ParseStatus);
            _emailMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ApproveBug_SetsApprovalStatusToApproved()
        {
            // Arrange
            var ticketId = 1;
            var ticket = new Ticket { Id = ticketId, ApprovalStatus = ApprovalStatus.PendingApproval };
            _ticketRepo.SetupAsyncQueryable(new List<Ticket> { ticket }.AsQueryable());
            _submissionRepo.SetupAsyncQueryable(new List<CustomerBugSubmission>().AsQueryable());

            // Act
            await _service.ApproveBugAsync(ticketId, new BugApprovalRequest(), 1);

            // Assert
            Assert.Equal(ApprovalStatus.Approved, ticket.ApprovalStatus);
            _ticketRepo.Verify(r => r.UpdateAsync(ticket), Times.Once);
        }
    }
}
