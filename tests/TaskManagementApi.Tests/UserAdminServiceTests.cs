using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Domain.Enums;
using TaskManagementApi.Application.DTOs.UserAdmin;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Identity;

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
        private readonly Mock<ISmsService> _smsMock;
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
            _smsMock = new Mock<ISmsService>();

            _service = new UserAdminService(
                _invitationRepo.Object, _userRepo.Object, _teamRepo.Object,
                _memberRepo.Object, _userManager.Object, _emailMock.Object, _smsMock.Object);
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
        public async Task VerifyMobile_ClearsCode_OnSuccess()
        {
            // Arrange
            var userId = 1;
            var code = "123456";
            var user = new User { Id = userId, MobileVerificationCode = code };
            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.VerifyMobileAsync(userId, code);

            // Assert
            Assert.True(result);
            Assert.Null(user.MobileVerificationCode);
            Assert.True(user.MobileVerified);
            _userRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task AcceptInvitation_Throws_WhenExpired()
        {
            // Arrange
            var token = Guid.NewGuid();
            var invitation = new UserInvitation { Token = token, Status = InvitationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddHours(-1) };
            _invitationRepo.Setup(r => r.Query()).Returns(new List<UserInvitation> { invitation }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AcceptInvitationAsync(new AcceptInvitationRequest { Token = token }));
        }
    }
}
