namespace TaskManagementApi.Application.Interfaces
{
    public interface IOtpService
    {
        Task SendOtpAsync(string mobileNumber);
        Task<bool> VerifyOtpAsync(string mobileNumber, string code, int userId);
    }
}
