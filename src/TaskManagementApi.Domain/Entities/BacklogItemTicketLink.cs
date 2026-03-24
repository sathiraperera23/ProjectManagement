namespace TaskManagementApi.Domain.Entities
{
    public class BacklogItemTicketLink
    {
        public int Id { get; set; }
        public int BacklogItemId { get; set; }
        public BacklogItem BacklogItem { get; set; } = null!;
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        public DateTime LinkedAt { get; set; }
        public int LinkedByUserId { get; set; }
    }
}
