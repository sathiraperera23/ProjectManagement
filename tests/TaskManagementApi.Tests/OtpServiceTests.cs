using Moq;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Infrastructure.Services;
using TaskManagementApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace TaskManagementApi.Tests
{
    public class OtpServiceTests
    {
        private readonly Mock<IRepository<MobileOtp>> _otpRepoMock;
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<ISmsService> _smsMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly OtpService _service;

        public OtpServiceTests()
        {
            _otpRepoMock = new Mock<IRepository<MobileOtp>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _smsMock = new Mock<ISmsService>();
            _configMock = new Mock<IConfiguration>();

            _service = new OtpService(_otpRepoMock.Object, _userRepoMock.Object, _smsMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task SendOtpAsync_DeactivatesExistingOtps()
        {
            // Arrange
            var mobile = "1234567890";
            var existing = new List<MobileOtp> { new MobileOtp { PhoneNumber = mobile, IsActive = true } }.AsQueryable();
            _otpRepoMock.Setup(r => r.Query()).Returns(existing);

            // Act
            await _service.SendOtpAsync(mobile);

            // Assert
            _otpRepoMock.Verify(r => r.UpdateAsync(It.Is<MobileOtp>(o => o.PhoneNumber == mobile && !o.IsActive)), Times.Once);
            _otpRepoMock.Verify(r => r.AddAsync(It.Is<MobileOtp>(o => o.PhoneNumber == mobile && o.IsActive)), Times.Once);
        }

        [Fact]
        public async Task VerifyOtpAsync_SetsUserMobileVerified_OnSuccess()
        {
            // Arrange
            var mobile = "1234567890";
            var code = "123456";
            var userId = 1;
            var otp = new MobileOtp { PhoneNumber = mobile, Code = 123456, IsActive = true, CreatedAt = DateTime.UtcNow };
            _otpRepoMock.Setup(r => r.Query()).Returns(new List<MobileOtp> { otp }.AsQueryable());

            var user = new User { Id = userId, MobileNumber = mobile };
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _service.VerifyOtpAsync(mobile, code, userId);

            // Assert
            Assert.True(result);
            Assert.True(user.MobileVerified);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task VerifyOtpAsync_Fails_WhenExpired()
        {
            // Arrange
            var mobile = "1234567890";
            var code = "123456";
            var otp = new MobileOtp { PhoneNumber = mobile, Code = 123456, IsActive = true, CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            _otpRepoMock.Setup(r => r.Query()).Returns(new List<MobileOtp> { otp }.AsQueryable());
            _configMock.Setup(c => c["Otp:SmsExpirationSeconds"]).Returns("300");

            // Act
            var result = await _service.VerifyOtpAsync(mobile, code, 1);

            // Assert
            Assert.False(result);
            Assert.False(otp.IsActive);
        }
    }
}
