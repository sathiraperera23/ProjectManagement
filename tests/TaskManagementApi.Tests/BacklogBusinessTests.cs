using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Application.DTOs.Backlog;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class BacklogBusinessTests
    {
        private readonly Mock<IRepository<BacklogItem>> _backlogRepoMock;
        private readonly Mock<IRepository<BacklogItemVersion>> _versionRepoMock;
        private readonly Mock<IRepository<BacklogAttachment>> _attachmentRepoMock;
        private readonly Mock<IRepository<BacklogItemTicketLink>> _ticketLinkRepoMock;
        private readonly Mock<IRepository<BacklogApprovalRequest>> _approvalRepoMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly BacklogService _service;

        public BacklogBusinessTests()
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
        public async Task UpdateAsync_IncrementsVersion_AndSetsStatusToDraft()
        {
            // Arrange
            var backlogId = 1;
            var item = new BacklogItem
            {
                Id = backlogId,
                Status = BacklogItemStatus.Approved,
                Title = "Old Title",
                Owner = new User { Id = 1, DisplayName = "Owner" }
            };

            _backlogRepoMock.Setup(r => r.GetByIdAsync(backlogId)).ReturnsAsync(item);
            _backlogRepoMock.SetupAsyncQueryable(new List<BacklogItem> { item }.AsQueryable());

            // Mock version query
            var versions = new List<BacklogItemVersion>
            {
                new BacklogItemVersion { BacklogItemId = backlogId, VersionNumber = 1 }
            }.AsQueryable();

            _versionRepoMock.SetupAsyncQueryable(versions);

            var request = new UpdateBacklogItemRequest
            {
                Title = "New Title",
                ChangeNote = "Updated title"
            };

            // Act
            await _service.UpdateAsync(backlogId, request, 1);

            // Assert
            Assert.Equal(BacklogItemStatus.Draft, item.Status);
            _versionRepoMock.Verify(v => v.AddAsync(It.Is<BacklogItemVersion>(bv => bv.VersionNumber == 2)), Times.Once);
        }

        [Fact]
        public async Task ApproveAsync_SetsStatusToApproved()
        {
            // Arrange
            var backlogId = 1;
            var approvalId = 10;
            var item = new BacklogItem { Id = backlogId, Status = BacklogItemStatus.Draft };
            var approval = new BacklogApprovalRequest
            {
                Id = approvalId,
                BacklogItemId = backlogId,
                Status = ApprovalRequestStatus.Pending
            };

            _backlogRepoMock.Setup(r => r.GetByIdAsync(backlogId)).ReturnsAsync(item);
            _approvalRepoMock.Setup(r => r.GetByIdAsync(approvalId)).ReturnsAsync(approval);

            // Act
            await _service.ApproveAsync(backlogId, approvalId, "Looks good", 1);

            // Assert
            Assert.Equal(BacklogItemStatus.Approved, item.Status);
            Assert.Equal(ApprovalRequestStatus.Approved, approval.Status);
        }
    }
}
