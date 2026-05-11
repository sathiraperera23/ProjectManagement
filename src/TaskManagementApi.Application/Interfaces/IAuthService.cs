using TaskManagementApi.Application.DTOs.Auth;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
