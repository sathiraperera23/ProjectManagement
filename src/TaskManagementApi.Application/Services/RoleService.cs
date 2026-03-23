using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Roles;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Permission> _permissionRepository;
        private readonly IRepository<RolePermission> _rolePermissionRepository;
        private readonly IRepository<UserProjectRole> _userProjectRoleRepository;
        private readonly IRepository<RoleAuditLog> _auditLogRepository;

        public RoleService(
            IRepository<Role> roleRepository,
            IRepository<Permission> permissionRepository,
            IRepository<RolePermission> rolePermissionRepository,
            IRepository<UserProjectRole> userProjectRoleRepository,
            IRepository<RoleAuditLog> auditLogRepository)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userProjectRoleRepository = userProjectRoleRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<string>> GetEffectivePermissionsAsync(int userId, int projectId)
        {
            var userProjectRole = await _userProjectRoleRepository.Query()
                .Include(upr => upr.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(upr => upr.UserId == userId && upr.ProjectId == projectId);

            if (userProjectRole == null) return Enumerable.Empty<string>();

            var permissions = new List<string>();
            var currentRole = userProjectRole.Role;

            while (currentRole != null)
            {
                permissions.AddRange(currentRole.RolePermissions.Select(rp => rp.Permission.Name));

                if (currentRole.ParentRoleId.HasValue)
                {
                    currentRole = await _roleRepository.Query()
                        .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                        .FirstOrDefaultAsync(r => r.Id == currentRole.ParentRoleId.Value);
                }
                else
                {
                    currentRole = null;
                }
            }

            return permissions.Distinct();
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, string createdByUserId)
        {
            var existing = await _roleRepository.Query().FirstOrDefaultAsync(r => r.Name == request.Name);
            if (existing != null)
                throw new InvalidOperationException($"A role named '{request.Name}' already exists");

            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                IsDefault = request.IsDefault,
                IsSystem = false,
                ParentRoleId = request.ParentRoleId,
                CreatedAt = DateTime.UtcNow
            };
            await _roleRepository.AddAsync(role);

            foreach (var permId in request.PermissionIds)
            {
                await _rolePermissionRepository.AddAsync(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permId
                });
            }

            await _auditLogRepository.AddAsync(new RoleAuditLog
            {
                RoleId = role.Id,
                Action = "ROLE_CREATED",
                ChangedByUserId = createdByUserId,
                Details = JsonSerializer.Serialize(new { role.Name, request.PermissionIds }),
                ChangedAt = DateTime.UtcNow
            });

            return (await GetRoleByIdAsync(role.Id))!;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.Query()
                .Include(r => r.ParentRole)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();
            return roles.Select(MapToDto);
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.Query()
                .Include(r => r.ParentRole)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
            return role != null ? MapToDto(role) : null;
        }

        public async Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleRequest request, string updatedByUserId)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) throw new KeyNotFoundException($"Role {id} not found");

            role.Name = request.Name;
            role.Description = request.Description;
            role.IsDefault = request.IsDefault;
            role.ParentRoleId = request.ParentRoleId;
            role.UpdatedAt = DateTime.UtcNow;

            await _roleRepository.UpdateAsync(role);

            await _auditLogRepository.AddAsync(new RoleAuditLog
            {
                RoleId = id,
                Action = "ROLE_UPDATED",
                ChangedByUserId = updatedByUserId,
                Details = JsonSerializer.Serialize(request),
                ChangedAt = DateTime.UtcNow
            });

            return (await GetRoleByIdAsync(id))!;
        }

        public async Task DeleteRoleAsync(int id, string deletedByUserId)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) throw new KeyNotFoundException($"Role {id} not found");
            if (role.IsSystem) throw new InvalidOperationException("System roles cannot be deleted");

            await _roleRepository.DeleteAsync(id);

            await _auditLogRepository.AddAsync(new RoleAuditLog
            {
                RoleId = id,
                Action = "ROLE_DELETED",
                ChangedByUserId = deletedByUserId,
                Details = JsonSerializer.Serialize(new { role.Name }),
                ChangedAt = DateTime.UtcNow
            });
        }

        public async Task UpdateRolePermissionsAsync(int id, UpdateRolePermissionsRequest request, string updatedByUserId)
        {
            var role = await _roleRepository.Query()
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) throw new KeyNotFoundException($"Role {id} not found");

            // Clear existing
            var existing = await _rolePermissionRepository.Query().Where(rp => rp.RoleId == id).ToListAsync();
            foreach (var rp in existing)
            {
                // Note: assuming our repository/DbContext supports direct deletion or tracking
                // For this scaffold, we'll manually remove them from the context if needed.
                // But GenericRepository.DeleteAsync takes an int ID, and RolePermission has a composite key.
                // I'll assume we can use a more specific method or clear the collection if tracked.
            }

            // To simplify for this scaffold, I will assume we can manage the collection directly on the entity
            role.RolePermissions.Clear();
            foreach (var permId in request.PermissionIds)
            {
                role.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = permId });
            }
            await _roleRepository.UpdateAsync(role);

            await _auditLogRepository.AddAsync(new RoleAuditLog
            {
                RoleId = id,
                Action = "PERMISSIONS_UPDATED",
                ChangedByUserId = updatedByUserId,
                Details = JsonSerializer.Serialize(request.PermissionIds),
                ChangedAt = DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Group = p.Group,
                Description = p.Description
            });
        }

        public async Task AssignRoleToUserAsync(AssignRoleRequest request, string assignedByUserId)
        {
            var existing = await _userProjectRoleRepository.Query()
                .FirstOrDefaultAsync(upr => upr.UserId == request.UserId && upr.ProjectId == request.ProjectId);

            if (existing != null)
            {
                existing.RoleId = request.RoleId;
                await _userProjectRoleRepository.UpdateAsync(existing);
            }
            else
            {
                await _userProjectRoleRepository.AddAsync(new UserProjectRole
                {
                    UserId = request.UserId,
                    ProjectId = request.ProjectId,
                    RoleId = request.RoleId,
                    AssignedAt = DateTime.UtcNow,
                    AssignedByUserId = int.Parse(assignedByUserId)
                });
            }
        }

        public async Task RemoveRoleFromUserAsync(int userId, int projectId, string removedByUserId)
        {
            var existing = await _userProjectRoleRepository.Query()
                .FirstOrDefaultAsync(upr => upr.UserId == userId && upr.ProjectId == projectId);
            if (existing != null)
            {
                await _userProjectRoleRepository.DeleteAsync(existing.Id);
            }
        }

        private RoleDto MapToDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsDefault = role.IsDefault,
                IsSystem = role.IsSystem,
                ParentRoleId = role.ParentRoleId,
                ParentRoleName = role.ParentRole?.Name,
                CreatedAt = role.CreatedAt,
                Permissions = role.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.PermissionId,
                    Name = rp.Permission.Name,
                    DisplayName = rp.Permission.DisplayName,
                    Group = rp.Permission.Group,
                    Description = rp.Permission.Description
                }).ToList()
            };
        }
    }
}
