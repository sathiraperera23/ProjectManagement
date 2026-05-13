using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Domain.Enums;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementApi.Infrastructure.Persistence;
using TaskManagementApi.Domain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskManagementApi.IntegrationTests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("sub", "keycloak-id-1")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class ProjectApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProjectApiTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                    // Grant all permissions for testing
                    services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
        }

        private class AllowAnonymousHandler : IAuthorizationHandler
        {
            public Task HandleAsync(AuthorizationHandlerContext context)
            {
                foreach (var requirement in context.PendingRequirements.ToList())
                    context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task CreateProject_ReturnsCreated()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
                if (!db.Users.Any(u => u.ProviderId == "keycloak-id-1"))
                {
                    db.Users.Add(new User
                    {
                        UserName = "test",
                        Email = "test@test.com",
                        Provider = "Keycloak",
                        ProviderId = "keycloak-id-1",
                        DisplayName = "Test User"
                    });
                    db.SaveChanges();
                }
            }

            var request = new CreateProjectRequest
            {
                Name = "Integration Test Project",
                ClientName = "Test Client",
                StartDate = DateTime.UtcNow,
                Status = ProjectStatus.Active,
                ProjectCode = "ITP"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/projects", request);

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Test failed. Status: {response.StatusCode}, Content: {content}");
            }

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
            Assert.NotNull(project);
            Assert.Equal("Integration Test Project", project.Name);
        }
    }
}
