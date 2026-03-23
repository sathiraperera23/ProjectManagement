namespace TaskManagementApi.Application.DTOs.Roles
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }
        public int? ParentRoleId { get; set; }
        public string? ParentRoleName { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Group { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDefault { get; set; }
        public int? ParentRoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDefault { get; set; }
        public int? ParentRoleId { get; set; }
    }

    public class UpdateRolePermissionsRequest
    {
        public List<int> PermissionIds { get; set; } = new();
    }

    public class AssignRoleRequest
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
    }
}
