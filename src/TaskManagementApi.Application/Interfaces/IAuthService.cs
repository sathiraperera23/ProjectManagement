using TaskManagementApi.Application.DTOs.Auth;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }
}
