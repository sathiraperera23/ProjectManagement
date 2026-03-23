namespace TaskManagementApi.Domain.Entities
{
    public class TicketAssignee : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public class TicketLabel : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public string Label { get; set; } = null!;
    }
}
