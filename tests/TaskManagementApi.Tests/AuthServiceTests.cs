using Microsoft.Extensions.Options;
using Moq;
using TaskManagementApi.Application.DTOs.Auth;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Infrastructure.Services;
using TaskManagementApi.Domain.Entities;
using Xunit;
using TaskManagementApi.Tests;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementApi.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IRepository<Role>> _roleRepoMock;
        private readonly Mock<IRepository<UserProjectRole>> _uprRepoMock;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IRepository<User>>();
            _roleRepoMock = new Mock<IRepository<Role>>();
            _uprRepoMock = new Mock<IRepository<UserProjectRole>>();

            _jwtSettings = Options.Create(new JwtSettings
            {
                SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                Issuer = "TaskManagementApi",
                Audience = "TaskManagementClient",
                AccessTokenExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            });

            _authService = new AuthService(
                _userRepoMock.Object,
                _roleRepoMock.Object,
                _uprRepoMock.Object,
                _jwtSettings);
        }

        [Fact]
        public async Task RegisterAsync_CreatesUserWithHashedPassword()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123!",
                DisplayName = "Test User"
            };

            User capturedUser = null!;
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedUser = u)
                .Returns(Task.CompletedTask);

            _userRepoMock.SetupAsyncQueryable(new List<User>().AsQueryable());
            _uprRepoMock.SetupAsyncQueryable(new List<UserProjectRole>().AsQueryable());

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            Assert.NotNull(capturedUser);
            Assert.Equal(request.Email, capturedUser.Email);
            Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash));
            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public async Task RegisterAsync_ThrowsWhenEmailAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequest { Email = "exists@example.com", Password = "Password1!", DisplayName = "Name" };
            var existingUser = new User { Email = "exists@example.com" };
            _userRepoMock.SetupAsyncQueryable(new List<User> { existingUser }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ReturnsTokensOnValidCredentials()
        {
            // Arrange
            var password = "Password123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Id = 1, Email = "test@example.com", PasswordHash = hashedPassword, IsActive = true, DisplayName = "Test" };

            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            _uprRepoMock.SetupAsyncQueryable(new List<UserProjectRole>().AsQueryable());

            var request = new LoginRequest { Email = "test@example.com", Password = password };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.Equal(user.Email, result.User.Email);
        }

        [Fact]
        public async Task LoginAsync_ThrowsOnInvalidPassword()
        {
            // Arrange
            var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPass"), IsActive = true };
            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            var request = new LoginRequest { Email = "test@example.com", Password = "WrongPass" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_ThrowsWhenAccountIsDeactivated()
        {
            // Arrange
            var user = new User { Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass"), IsActive = false };
            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            var request = new LoginRequest { Email = "test@example.com", Password = "Pass" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }

        [Fact]
        public async Task RefreshAsync_ReturnsNewTokensOnValidRefreshToken()
        {
            // Arrange
            var token = "valid_refresh_token";
            var user = new User { Id = 1, Email = "test@example.com", RefreshToken = token, RefreshTokenExpiry = DateTime.UtcNow.AddDays(1), IsActive = true, DisplayName = "Test" };
            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());
            _uprRepoMock.SetupAsyncQueryable(new List<UserProjectRole>().AsQueryable());

            // Act
            var result = await _authService.RefreshAsync(token);

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotEqual(token, result.RefreshToken);
        }

        [Fact]
        public async Task RefreshAsync_ThrowsWhenRefreshTokenHasExpired()
        {
            // Arrange
            var token = "expired_token";
            var user = new User { Email = "test@example.com", RefreshToken = token, RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1) };
            _userRepoMock.SetupAsyncQueryable(new List<User> { user }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshAsync(token));
        }
    }
}
