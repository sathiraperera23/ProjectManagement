namespace TaskManagementApi.Domain.Entities
{
    public class BacklogItemVersion
    {
        public int Id { get; set; }
        public int BacklogItemId { get; set; }
        public BacklogItem BacklogItem { get; set; } = null!;
        public int VersionNumber { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? AcceptanceCriteria { get; set; }
        public int CreatedByUserId { get; set; }
        public User CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string ChangeNote { get; set; } = null!;
    }
}
