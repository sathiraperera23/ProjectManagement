using System.Text.Json.Serialization;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IKeycloakAuthService
    {
        Task<TokenResponse> LoginAsync(string username, string password);
        Task<TokenResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = null!;
    }
}
