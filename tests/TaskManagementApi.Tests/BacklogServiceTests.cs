using Microsoft.AspNetCore.Http;
using Moq;
using TaskManagementApi.Application.DTOs.Backlog;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Tests
{
    public class BacklogServiceTests
    {
        private readonly Mock<IRepository<BacklogItem>> _backlogRepoMock;
        private readonly Mock<IRepository<BacklogItemVersion>> _versionRepoMock;
        private readonly Mock<IRepository<BacklogAttachment>> _attachmentRepoMock;
        private readonly Mock<IRepository<BacklogItemTicketLink>> _ticketLinkRepoMock;
        private readonly Mock<IRepository<BacklogApprovalRequest>> _approvalRepoMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly BacklogService _service;

        public BacklogServiceTests()
        {
            _backlogRepoMock = new Mock<IRepository<BacklogItem>>();
            _versionRepoMock = new Mock<IRepository<BacklogItemVersion>>();
            _attachmentRepoMock = new Mock<IRepository<BacklogAttachment>>();
            _ticketLinkRepoMock = new Mock<IRepository<BacklogItemTicketLink>>();
            _approvalRepoMock = new Mock<IRepository<BacklogApprovalRequest>>();
            _fileStorageMock = new Mock<IFileStorageService>();

            _service = new BacklogService(
                _backlogRepoMock.Object,
                _versionRepoMock.Object,
                _attachmentRepoMock.Object,
                _ticketLinkRepoMock.Object,
                _approvalRepoMock.Object,
                _fileStorageMock.Object);
        }

        [Fact]
        public void Service_CanBeInitialized()
        {
            Assert.NotNull(_service);
        }

        [Fact]
        public async Task AddAttachmentAsync_ThrowsWhenFileExceeds25MB()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(26 * 1024 * 1024);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAttachmentAsync(1, fileMock.Object, 1));
        }
    }
}
