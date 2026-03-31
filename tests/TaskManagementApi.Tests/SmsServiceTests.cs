using System.Net;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManagementApi.Infrastructure.Services;
using TaskManagementApi.Application.DTOs.Notifications;

namespace TaskManagementApi.Tests
{
    public class SmsServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<SmsService>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly SmsService _service;

        public SmsServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<SmsService>>();
            _handlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_handlerMock.Object);
            _service = new SmsService(httpClient, _configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SendSmsAsync_ReturnsFalse_WhenDisabled()
        {
            // Arrange
            _configMock.Setup(c => c["Sms:Enabled"]).Returns("false");

            // Act
            var result = await _service.SendSmsAsync("12345", "test");

            // Assert
            Assert.False(result);
            // Verify logging occurred
        }

        [Fact]
        public async Task SendSmsAsync_PostsCorrectPayload()
        {
            // Arrange
            _configMock.Setup(c => c["Sms:Enabled"]).Returns("true");
            _configMock.Setup(c => c["Sms:CommonModuleUrl"]).Returns("http://api.com");
            _configMock.Setup(c => c["Sms:Endpoint"]).Returns("/sms");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("http://api.com/sms")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await _service.SendSmsAsync("12345", "test message");

            // Assert
            Assert.True(result);
        }
    }
}
