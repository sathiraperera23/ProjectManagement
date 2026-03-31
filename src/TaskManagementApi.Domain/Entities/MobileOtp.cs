namespace TaskManagementApi.Domain.Entities
{
    public class MobileOtp
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public int Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
    }
}
