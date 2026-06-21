using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Application.DTOs.Auth;
using TaskManagementApi.Application.DTOs.Projects;
using TaskManagementApi.Domain.Enums;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskManagementApi.IntegrationTests
{
    public class AdminSeederTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AdminSeederTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task AdminSeeder_CreatesUserAndAllowsProjectCreation()
        {
            // Arrange
            // Ensure environment is Development for the seeder to run
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            }).CreateClient();

            // 1. Verify Login
            var loginRequest = new LoginRequest
            {
                Email = "admin@admin.com",
                Password = "Admin@123"
            };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert Login
            var content = await loginResponse.Content.ReadAsStringAsync();
            Assert.True(HttpStatusCode.OK == loginResponse.StatusCode, $"Login failed with {loginResponse.StatusCode}: {content}");
            var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            Assert.NotNull(authResult);
            Assert.NotNull(authResult.AccessToken);
            Assert.Contains("Admin", authResult.User.Roles);

            // 2. Use token to create a project
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);

            var projectRequest = new CreateProjectRequest
            {
                Name = "Admin Seeded Project",
                ClientName = "Seeder Client",
                StartDate = DateTime.UtcNow,
                Status = ProjectStatus.Active,
                ProjectCode = "ASP"
            };

            var projectResponse = await client.PostAsJsonAsync("/api/projects", projectRequest);

            // Assert Project Creation
            Assert.Equal(HttpStatusCode.Created, projectResponse.StatusCode);
            var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
            Assert.NotNull(project);
            Assert.Equal("Admin Seeded Project", project.Name);
        }
    }
}
