using Microsoft.AspNetCore.Http;
using Moq;
using TaskManagementApi.Application.DTOs.Tickets;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class TicketExtraServiceTests
    {
        private readonly Mock<IRepository<TicketComment>> _commentRepoMock;
        private readonly Mock<IRepository<CommentMention>> _mentionRepoMock;
        private readonly Mock<IRepository<CommentReaction>> _reactionRepoMock;
        private readonly Mock<IRepository<TicketWatcher>> _watcherRepoMock;
        private readonly Mock<IRepository<TicketAttachment>> _attachmentRepoMock;
        private readonly Mock<IRepository<DailyUpdate>> _dailyUpdateRepoMock;
        private readonly Mock<IRepository<DailyUpdateTicketLink>> _linkRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly TicketExtraService _service;

        public TicketExtraServiceTests()
        {
            _commentRepoMock = new Mock<IRepository<TicketComment>>();
            _mentionRepoMock = new Mock<IRepository<CommentMention>>();
            _reactionRepoMock = new Mock<IRepository<CommentReaction>>();
            _watcherRepoMock = new Mock<IRepository<TicketWatcher>>();
            _attachmentRepoMock = new Mock<IRepository<TicketAttachment>>();
            _dailyUpdateRepoMock = new Mock<IRepository<DailyUpdate>>();
            _linkRepoMock = new Mock<IRepository<DailyUpdateTicketLink>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _fileStorageMock = new Mock<IFileStorageService>();

            _service = new TicketExtraService(
                _commentRepoMock.Object, _mentionRepoMock.Object, _reactionRepoMock.Object,
                _watcherRepoMock.Object, _attachmentRepoMock.Object, _dailyUpdateRepoMock.Object,
                _linkRepoMock.Object, _userRepoMock.Object, _fileStorageMock.Object);
        }

        [Fact]
        public async Task PostCommentAsync_ParsesMentions()
        {
            // Arrange
            var ticketId = 1;
            var userId = 100;
            var request = new CreateCommentRequest { Body = "Hello @123 and @456", IsInternalNote = false };

            _commentRepoMock.SetupAsyncQueryable(new List<TicketComment>().AsQueryable());

            // Act
            await _service.PostCommentAsync(ticketId, request, userId);

            // Assert
            _mentionRepoMock.Verify(r => r.AddAsync(It.Is<CommentMention>(m => m.UserId == 123)), Times.Once);
            _mentionRepoMock.Verify(r => r.AddAsync(It.Is<CommentMention>(m => m.UserId == 456)), Times.Once);
        }

        [Fact]
        public async Task UploadAttachmentAsync_IncrementsVersion_OnReupload()
        {
            // Arrange
            var ticketId = 1;
            var fileName = "test.txt";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            var existing = new List<TicketAttachment> {
                new TicketAttachment { TicketId = ticketId, FileName = fileName, Version = 1 }
            }.AsQueryable();
            _attachmentRepoMock.SetupAsyncQueryable(existing);

            // Act
            var result = await _service.UploadAttachmentAsync(ticketId, fileMock.Object, 1);

            // Assert
            Assert.Equal(2, result.Version);
        }

        [Fact]
        public async Task PostDailyUpdateAsync_EnforcesOnePerDay()
        {
            // Arrange
            var userId = 1;
            var request = new CreateDailyUpdateRequest { ProjectId = 1, WorkedOn = "Code", PlannedNext = "Test" };
            var existing = new DailyUpdate { UserId = userId, SubmittedAt = DateTime.UtcNow.Date };

            _dailyUpdateRepoMock.SetupAsyncQueryable(new List<DailyUpdate> { existing }.AsQueryable());

            // Act
            await _service.PostDailyUpdateAsync(request, userId);

            // Assert
            _dailyUpdateRepoMock.Verify(r => r.AddAsync(It.IsAny<DailyUpdate>()), Times.Never);
            _dailyUpdateRepoMock.Verify(r => r.UpdateAsync(It.IsAny<DailyUpdate>()), Times.Once);
        }

        [Fact]
        public async Task UploadAttachmentAsync_Throws_WhenImageTooLarge()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11MB

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UploadAttachmentAsync(1, fileMock.Object, 1));
        }
    }
}
