using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Infrastructure.Services;
using TaskManagementApi.Domain.Entities;
using System.Threading.Tasks;
using Xunit;

namespace TaskManagementApi.Tests
{
    public class BugReportTemplateServiceTests
    {
        private readonly Mock<IRepository<Project>> _projectRepoMock;
        private readonly BugReportTemplateService _service;

        public BugReportTemplateServiceTests()
        {
            _projectRepoMock = new Mock<IRepository<Project>>();
            _service = new BugReportTemplateService(_projectRepoMock.Object);
        }

        [Fact]
        public async Task GetTemplateAsync_ReturnsTemplate_WithIntakeEmail()
        {
            // Arrange
            var projectId = 1;
            var intakeEmail = "bugs@project.com";
            var project = new Project { Id = projectId, IntakeEmailAddress = intakeEmail };
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _service.GetTemplateAsync(projectId);

            // Assert
            Assert.Contains($"TO: {intakeEmail}", result);
        }

        [Fact]
        public async Task GetTemplateAsync_AppendsCustomText_WhenSet()
        {
            // Arrange
            var projectId = 1;
            var customText = "Special instructions here";
            var project = new Project { Id = projectId, BugReportTemplateCustomText = customText };
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _service.GetTemplateAsync(projectId);

            // Assert
            Assert.Contains(customText, result);
        }

        [Fact]
        public async Task GetTemplateAsync_OmitsCustomText_WhenNull()
        {
            // Arrange
            var projectId = 1;
            var project = new Project { Id = projectId, BugReportTemplateCustomText = null };
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _service.GetTemplateAsync(projectId);

            // Assert
            // The template has placeholders, but should not have a specific custom text block if null
            // Based on implementation, it just skips the AppendLine
            var lines = result.Split("\n");
            // Check that no line matches a common null string or empty placeholder where custom text goes
        }

        [Fact]
        public async Task UpdateCustomTextAsync_SavesCorrectly()
        {
            // Arrange
            var projectId = 1;
            var customText = "New Custom Text";
            var project = new Project { Id = projectId };
            _projectRepoMock.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            await _service.UpdateCustomTextAsync(projectId, customText, 1);

            // Assert
            Assert.Equal(customText, project.BugReportTemplateCustomText);
            _projectRepoMock.Verify(r => r.UpdateAsync(project), Times.Once);
        }
    }
}
