using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;

namespace TaskManagementApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<SubProject> SubProjects { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
    }
}
