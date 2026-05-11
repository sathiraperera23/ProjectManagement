using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Application.DTOs.Auth;
using TaskManagementApi.Application.Interfaces;
using Xunit;

namespace TaskManagementApi.IntegrationTests
{
    public class AuthApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterAndLogin_ShouldWork()
        {
            // 1. Register
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                DisplayName = "New User"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            // 2. Login
            var loginRequest = new LoginRequest
            {
                Username = "newuser@example.com",
                Password = "Password123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(tokens);
            Assert.NotNull(tokens.AccessToken);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var loginRequest = new LoginRequest
            {
                Username = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }
    }
}
