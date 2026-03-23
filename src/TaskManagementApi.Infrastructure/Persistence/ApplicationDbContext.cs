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
        public DbSet<TicketStatus> TicketStatuses { get; set; } = null!;
        public DbSet<TicketStatusHistory> TicketStatusHistories { get; set; } = null!;
        public DbSet<TicketLink> TicketLinks { get; set; } = null!;
        public DbSet<TicketAssignee> TicketAssignees { get; set; } = null!;
        public DbSet<TicketLabel> TicketLabels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<SubProject>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Ticket>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<TicketStatus>().HasQueryFilter(e => !e.IsDeleted);

            builder.Entity<Project>()
                .HasIndex(p => p.ProjectCode)
                .IsUnique();

            builder.Entity<SubProject>()
                .HasOne(s => s.DependsOnSubProject)
                .WithMany()
                .HasForeignKey(s => s.DependsOnSubProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket Links
            builder.Entity<TicketLink>()
                .HasOne(tl => tl.SourceTicket)
                .WithMany(t => t.OutgoingLinks)
                .HasForeignKey(tl => tl.SourceTicketId);

            builder.Entity<TicketLink>()
                .HasOne(tl => tl.TargetTicket)
                .WithMany(t => t.IncomingLinks)
                .HasForeignKey(tl => tl.TargetTicketId);

            // Ticket Status History
            builder.Entity<TicketStatusHistory>()
                .HasOne(h => h.FromStatus)
                .WithMany()
                .HasForeignKey(h => h.FromStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TicketStatusHistory>()
                .HasOne(h => h.ToStatus)
                .WithMany()
                .HasForeignKey(h => h.ToStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
