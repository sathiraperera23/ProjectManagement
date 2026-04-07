using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Application.DTOs.UserAdmin;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TaskManagementApi.Tests
{
    public class UserAdminServiceTests
    {
        private readonly Mock<IRepository<UserInvitation>> _invitationRepo;
        private readonly Mock<IRepository<User>> _userRepo;
        private readonly Mock<IRepository<Team>> _teamRepo;
        private readonly Mock<IRepository<TeamMember>> _memberRepo;
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<IEmailService> _emailMock;
        private readonly Mock<IOtpService> _otpMock;
        private readonly UserAdminService _service;

        public UserAdminServiceTests()
        {
            _invitationRepo = new Mock<IRepository<UserInvitation>>();
            _userRepo = new Mock<IRepository<User>>();
            _teamRepo = new Mock<IRepository<Team>>();
            _memberRepo = new Mock<IRepository<TeamMember>>();

            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _emailMock = new Mock<IEmailService>();
            _otpMock = new Mock<IOtpService>();

            _service = new UserAdminService(
                _invitationRepo.Object, _userRepo.Object, _teamRepo.Object,
                _memberRepo.Object, _userManager.Object, _emailMock.Object, _otpMock.Object);
        }

        [Fact]
        public async Task InviteUser_SetsCorrectExpiry()
        {
            // Arrange
            var request = new InviteUserRequest { Email = "test@user.com" };

            // Act
            await _service.InviteUserAsync(request, 1);

            // Assert
            _invitationRepo.Verify(r => r.AddAsync(It.Is<UserInvitation>(i =>
                i.Email == request.Email &&
                i.ExpiresAt > DateTime.UtcNow.AddHours(47))), Times.Once);
        }

        [Fact]
        public async Task VerifyMobile_CallsOtpService()
        {
            // Arrange
            var userId = 1;
            var code = "123456";
            var user = new User { Id = userId, MobileNumber = "1234567890" };
            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _otpMock.Setup(s => s.VerifyOtpAsync(user.MobileNumber, code, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.VerifyMobileAsync(userId, code);

            // Assert
            Assert.True(result);
            _otpMock.Verify(s => s.VerifyOtpAsync(user.MobileNumber, code, userId), Times.Once);
        }

        [Fact]
        public async Task AcceptInvitation_Throws_WhenExpired()
        {
            // Arrange
            var token = Guid.NewGuid();
            // UserInvitation doesn't have CreatedAt anymore, using BaseEntity one
            var invitation = new UserInvitation { Token = token, Status = InvitationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddHours(-1) };

            // Mocking IQueryable for GenericRepository.Query()
            var data = new List<UserInvitation> { invitation }.AsQueryable();
            _invitationRepo.Setup(r => r.Query()).Returns(data);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AcceptInvitationAsync(new AcceptInvitationRequest { Token = token }));
        }
    }
}
