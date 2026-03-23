using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<SubProject>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Ticket>().HasQueryFilter(e => !e.IsDeleted);

            builder.Entity<Project>()
                .HasIndex(p => p.ProjectCode)
                .IsUnique();

            builder.Entity<SubProject>()
                .HasOne(s => s.DependsOnSubProject)
                .WithMany()
                .HasForeignKey(s => s.DependsOnSubProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
