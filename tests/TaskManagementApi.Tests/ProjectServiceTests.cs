using Moq;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Tests
{
    public class ProjectServiceTests
    {
        [Theory]
        [InlineData("User Management System", "USE")]
        [InlineData("AI", "AIX")]
        [InlineData("Task API", "TAS")]
        public async Task CreateProjectAsync_GeneratesCorrectProjectCode(string projectName, string expectedCode)
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Project>>();
            var mockStatusRepo = new Mock<IRepository<TicketStatus>>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
            var service = new ProjectService(mockRepo.Object, mockStatusRepo.Object);
            var request = new CreateProjectRequest
            {
                Name = projectName,
                StartDate = DateTime.Now,
                Status = ProjectStatus.Active
            };

            // Act
            var result = await service.CreateProjectAsync(request);

            // Assert
            Assert.Equal(expectedCode, result.ProjectCode);
        }

        [Fact]
        public async Task ArchiveProjectAsync_SetsIsArchivedToTrue()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Project>>();
            var mockStatusRepo = new Mock<IRepository<TicketStatus>>();
            var project = new Project { Id = 1, IsArchived = false };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
            var service = new ProjectService(mockRepo.Object, mockStatusRepo.Object);

            // Act
            await service.ArchiveProjectAsync(1);

            // Assert
            Assert.True(project.IsArchived);
        }
    }
}
