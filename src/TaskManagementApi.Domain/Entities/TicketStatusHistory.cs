namespace TaskManagementApi.Domain.Entities
{
    public class TicketStatusHistory : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public int FromStatusId { get; set; }
        public TicketStatus FromStatus { get; set; } = null!;
        public int ToStatusId { get; set; }
        public TicketStatus ToStatus { get; set; } = null!;
        public int ChangedByUserId { get; set; }
        public User ChangedByUser { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}
