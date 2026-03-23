using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.DTOs.Products
{
    public class CreateProductRequest
    {
        public string VersionName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime PlannedReleaseDate { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public ProductStatus Status { get; set; }
    }

    public class UpdateProductRequest
    {
        public string VersionName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime PlannedReleaseDate { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public ProductStatus Status { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string VersionName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime PlannedReleaseDate { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public ProductStatus Status { get; set; }
    }
}
