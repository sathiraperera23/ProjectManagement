using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class TicketLink : BaseEntity
    {
        public int SourceTicketId { get; set; }
        public Ticket SourceTicket { get; set; } = null!;
        public int TargetTicketId { get; set; }
        public Ticket TargetTicket { get; set; } = null!;
        public TicketLinkType LinkType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
