using Moq;
using TaskManagementApi.Application.DTOs.Roles;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Tests
{
    public class RoleServiceTests
    {
        private readonly Mock<IRepository<Role>> _roleRepoMock;
        private readonly Mock<IRepository<Permission>> _permRepoMock;
        private readonly Mock<IRepository<RolePermission>> _rolePermRepoMock;
        private readonly Mock<IRepository<UserProjectRole>> _uprRepoMock;
        private readonly Mock<IRepository<RoleAuditLog>> _auditRepoMock;
        private readonly RoleService _service;

        public RoleServiceTests()
        {
            _roleRepoMock = new Mock<IRepository<Role>>();
            _permRepoMock = new Mock<IRepository<Permission>>();
            _rolePermRepoMock = new Mock<IRepository<RolePermission>>();
            _uprRepoMock = new Mock<IRepository<UserProjectRole>>();
            _auditRepoMock = new Mock<IRepository<RoleAuditLog>>();

            _service = new RoleService(
                _roleRepoMock.Object,
                _permRepoMock.Object,
                _rolePermRepoMock.Object,
                _uprRepoMock.Object,
                _auditRepoMock.Object);
        }

        [Fact]
        public void Service_CanBeInitialized()
        {
            Assert.NotNull(_service);
        }

        [Fact]
        public async Task DeleteRoleAsync_ThrowsWhenSystemRole()
        {
            // Arrange
            var systemRole = new Role { IsSystem = true };
            systemRole.Id = 1;
            _roleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(systemRole);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteRoleAsync(1, "1"));
        }
    }
}
