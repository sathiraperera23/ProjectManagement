using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Infrastructure.Services
{
    public class KeycloakAuthService : IKeycloakAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public KeycloakAuthService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private string TokenUrl => $"{_config["Keycloak:AuthServerUrl"]}/realms/" +
                                   $"{_config["Keycloak:Realm"]}/protocol/openid-connect/token";

        private string LogoutUrl => $"{_config["Keycloak:AuthServerUrl"]}/realms/" +
                                    $"{_config["Keycloak:Realm"]}/protocol/openid-connect/logout";

        public async Task<TokenResponse> LoginAsync(string username, string password)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _config["Keycloak:ClientId"] ?? "",
                ["client_secret"] = _config["Keycloak:ClientSecret"] ?? "",
                ["username"] = username,
                ["password"] = password
            };
            var response = await _httpClient.PostAsync(TokenUrl,
                               new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task<TokenResponse> RefreshAsync(string refreshToken)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _config["Keycloak:ClientId"] ?? "",
                ["client_secret"] = _config["Keycloak:ClientSecret"] ?? "",
                ["refresh_token"] = refreshToken
            };
            var response = await _httpClient.PostAsync(TokenUrl,
                               new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var form = new Dictionary<string, string>
            {
                ["client_id"] = _config["Keycloak:ClientId"] ?? "",
                ["client_secret"] = _config["Keycloak:ClientSecret"] ?? "",
                ["refresh_token"] = refreshToken
            };
            await _httpClient.PostAsync(LogoutUrl,
                new FormUrlEncodedContent(form));
        }
    }
}
