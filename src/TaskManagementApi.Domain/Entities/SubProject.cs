using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Domain.Entities
{
    public class SubProject : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public SubProjectStatus Status { get; set; }
        public int? ModuleOwnerUserId { get; set; }

        public int? DependsOnSubProjectId { get; set; }
        public SubProject? DependsOnSubProject { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
