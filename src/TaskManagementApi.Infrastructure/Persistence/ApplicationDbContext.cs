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
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserProjectRole> UserProjectRoles { get; set; } = null!;
        public DbSet<RoleAuditLog> RoleAuditLogs { get; set; } = null!;

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

        public DbSet<BacklogItem> BacklogItems => Set<BacklogItem>();
        public DbSet<BacklogItemVersion> BacklogItemVersions => Set<BacklogItemVersion>();
        public DbSet<BacklogAttachment> BacklogAttachments => Set<BacklogAttachment>();
        public DbSet<BacklogItemTicketLink> BacklogItemTicketLinks => Set<BacklogItemTicketLink>();
        public DbSet<BacklogApprovalRequest> BacklogApprovalRequests => Set<BacklogApprovalRequest>();

        public DbSet<Sprint> Sprints => Set<Sprint>();
        public DbSet<SprintMemberCapacity> SprintMemberCapacities => Set<SprintMemberCapacity>();
        public DbSet<SprintScopeChange> SprintScopeChanges => Set<SprintScopeChange>();

        public DbSet<TicketComment> TicketComments => Set<TicketComment>();
        public DbSet<CommentMention> CommentMentions => Set<CommentMention>();
        public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();
        public DbSet<TicketWatcher> TicketWatchers => Set<TicketWatcher>();
        public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
        public DbSet<DailyUpdate> DailyUpdates => Set<DailyUpdate>();
        public DbSet<DailyUpdateTicketLink> DailyUpdateTicketLinks => Set<DailyUpdateTicketLink>();

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
        public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();

        public DbSet<ProjectBudget> ProjectBudgets => Set<ProjectBudget>();
        public DbSet<UserRate> UserRates => Set<UserRate>();
        public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
        public DbSet<DelayRecord> DelayRecords => Set<DelayRecord>();
        public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();

        public DbSet<CustomerBugSubmission> CustomerBugSubmissions => Set<CustomerBugSubmission>();
        public DbSet<BugApprovalSla> BugApprovalSlas => Set<BugApprovalSla>();

        public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<MobileOtp> MobileOtps => Set<MobileOtp>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<SubProject>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Ticket>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<TicketStatus>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<BacklogItem>().HasQueryFilter(b => !b.IsDeleted);
            builder.Entity<Sprint>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<TicketComment>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<CommentReaction>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<TicketAttachment>().HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<DailyUpdate>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Notification>().HasQueryFilter(n => !n.IsDeleted);
            builder.Entity<NotificationPreference>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<NotificationLog>().HasQueryFilter(l => !l.IsDeleted);
            builder.Entity<NotificationRule>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<ProjectBudget>().HasQueryFilter(b => !b.IsDeleted);
            builder.Entity<UserRate>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<TimeLog>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<DelayRecord>().HasQueryFilter(d => !d.IsDeleted);
            builder.Entity<CustomerBugSubmission>().HasQueryFilter(b => !b.IsDeleted);
            builder.Entity<BugApprovalSla>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<UserInvitation>().HasQueryFilter(i => !i.IsDeleted);
            builder.Entity<Team>().HasQueryFilter(t => !t.IsDeleted);

            builder.Entity<Project>().HasIndex(p => p.ProjectCode).IsUnique();

            builder.Entity<SubProject>()
                .HasOne(s => s.DependsOnSubProject)
                .WithMany()
                .HasForeignKey(s => s.DependsOnSubProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TicketLink>()
                .HasOne(tl => tl.SourceTicket)
                .WithMany(t => t.OutgoingLinks)
                .HasForeignKey(tl => tl.SourceTicketId);

            builder.Entity<TicketLink>()
                .HasOne(tl => tl.TargetTicket)
                .WithMany(t => t.IncomingLinks)
                .HasForeignKey(tl => tl.TargetTicketId);

            builder.Entity<TicketStatusHistory>()
                .HasOne(h => h.FromStatus).WithMany().HasForeignKey(h => h.FromStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TicketStatusHistory>()
                .HasOne(h => h.ToStatus).WithMany().HasForeignKey(h => h.ToStatusId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            builder.Entity<UserProjectRole>().HasIndex(upr => new { upr.UserId, upr.ProjectId }).IsUnique();

            builder.Entity<Role>().HasMany(r => r.RolePermissions).WithOne(rp => rp.Role).HasForeignKey(rp => rp.RoleId);
            builder.Entity<Permission>().HasMany<RolePermission>().WithOne(rp => rp.Permission).HasForeignKey(rp => rp.PermissionId);

            builder.Entity<BacklogItem>().HasOne(b => b.Project).WithMany().HasForeignKey(b => b.ProjectId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BacklogItem>().HasOne(b => b.Product).WithMany().HasForeignKey(b => b.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<BacklogItemTicketLink>().HasIndex(b => new { b.BacklogItemId, b.TicketId }).IsUnique();

            builder.Entity<CommentMention>().HasKey(m => new { m.CommentId, m.UserId });
            builder.Entity<TicketWatcher>().HasKey(w => new { w.TicketId, w.UserId });
            builder.Entity<DailyUpdateTicketLink>().HasKey(l => new { l.DailyUpdateId, l.TicketId });
            builder.Entity<TeamMember>().HasKey(m => new { m.TeamId, m.UserId });

            builder.Entity<TicketComment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
