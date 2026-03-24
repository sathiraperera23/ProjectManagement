namespace TaskManagementApi.Domain.Entities
{
    public class BacklogAttachment
    {
        public int Id { get; set; }
        public int BacklogItemId { get; set; }
        public BacklogItem BacklogItem { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public int UploadedByUserId { get; set; }
        public User UploadedBy { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
