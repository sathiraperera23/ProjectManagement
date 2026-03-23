using System.Net;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Infrastructure.Services;

namespace TaskManagementApi.Tests
{
    public class KeycloakAuthServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configMock;
        private readonly KeycloakAuthService _service;

        public KeycloakAuthServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object);
            _configMock = new Mock<IConfiguration>();

            _configMock.Setup(c => c["Keycloak:AuthServerUrl"]).Returns("https://keycloak");
            _configMock.Setup(c => c["Keycloak:Realm"]).Returns("test-realm");
            _configMock.Setup(c => c["Keycloak:ClientId"]).Returns("test-client");
            _configMock.Setup(c => c["Keycloak:ClientSecret"]).Returns("test-secret");

            _service = new KeycloakAuthService(_httpClient, _configMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ReturnsTokenResponse_OnSuccess()
        {
            // Arrange
            var jsonResponse = "{\"access_token\": \"abc\", \"refresh_token\": \"def\", \"expires_in\": 300, \"token_type\": \"Bearer\"}";
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });

            // Act
            var result = await _service.LoginAsync("user", "pass");

            // Assert
            Assert.Equal("abc", result.AccessToken);
            Assert.Equal("def", result.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_ThrowsHttpRequestException_OnUnauthorized()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.LoginAsync("user", "pass"));
        }

        [Fact]
        public async Task RefreshAsync_ReturnsNewTokens_OnValidRefreshToken()
        {
            // Arrange
            var jsonResponse = "{\"access_token\": \"new_abc\", \"refresh_token\": \"new_def\", \"expires_in\": 300, \"token_type\": \"Bearer\"}";
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });

            // Act
            var result = await _service.RefreshAsync("old_refresh");

            // Assert
            Assert.Equal("new_abc", result.AccessToken);
        }

        [Fact]
        public async Task LogoutAsync_CompletesSuccessfully_OnValidRequest()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            // Act
            await _service.LogoutAsync("refresh_token");

            // Assert
            _handlerMock.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}
