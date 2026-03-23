using Microsoft.AspNetCore.Identity;

namespace TaskManagementApi.Domain.Entities
{
    public class Role : IdentityRole<int>
    {
        public string Description { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }      // true for built-in roles — cannot be deleted
        public int? ParentRoleId { get; set; }  // for role inheritance
        public Role? ParentRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserProjectRole> UserProjectRoles { get; set; } = new List<UserProjectRole>();
    }
}
