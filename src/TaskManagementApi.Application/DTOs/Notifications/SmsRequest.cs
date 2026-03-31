namespace TaskManagementApi.Application.DTOs.Notifications
{
    public class SmsRequest
    {
        public string Mobile { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
