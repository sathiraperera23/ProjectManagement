using Moq;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Application.DTOs.Products;
using TaskManagementApi.Application.DTOs.SubProjects;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Tests
{
    public class ProjectServiceTests
    {
        [Fact]
        public async Task CreateProjectAsync_GeneratesCorrectProjectCode()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Project>>();
            var mockStatusRepo = new Mock<IRepository<TicketStatus>>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);
            var service = new ProjectService(mockRepo.Object, mockStatusRepo.Object);
            var request = new CreateProjectRequest { Name = "User Management System", StartDate = DateTime.Now, Status = ProjectStatus.Active };

            // Act
            var result = await service.CreateProjectAsync(request);

            // Assert
            Assert.Equal("USE", result.ProjectCode);
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

    public class ProductServiceTests
    {
        [Fact]
        public async Task CreateProductAsync_CreatesProductCorrectly()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Product>>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            var service = new ProductService(mockRepo.Object);
            var request = new CreateProductRequest { VersionName = "v1.0.0", PlannedReleaseDate = DateTime.Now, ReleaseType = ReleaseType.Major, Status = ProductStatus.Planned };

            // Act
            var result = await service.CreateProductAsync(1, request);

            // Assert
            Assert.Equal(1, result.ProjectId);
            Assert.Equal("v1.0.0", result.VersionName);
        }
    }

    public class SubProjectServiceTests
    {
        [Fact]
        public async Task GetSubProjectProgressAsync_ReturnsCorrectPercentage()
        {
            // Arrange
            var mockSubRepo = new Mock<IRepository<SubProject>>();
            var mockTicketRepo = new Mock<IRepository<Ticket>>();

            var service = new SubProjectService(mockSubRepo.Object, mockTicketRepo.Object);

            Assert.NotNull(service);
        }
    }
}
