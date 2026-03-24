using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Infrastructure.Persistence;

namespace TaskManagementApi.Infrastructure
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Permissions.AnyAsync()) return;

            // Seed all 31 permissions
            var permissions = new List<Permission>
            {
                new() { Name = "CREATE_PROJECT",   DisplayName = "Create Project",   Group = "Project", Description = "Allows creating new projects" },
                new() { Name = "EDIT_PROJECT",     DisplayName = "Edit Project",     Group = "Project", Description = "Allows editing project details" },
                new() { Name = "ARCHIVE_PROJECT",  DisplayName = "Archive Project",  Group = "Project", Description = "Allows archiving projects" },
                new() { Name = "DELETE_PROJECT",   DisplayName = "Delete Project",   Group = "Project", Description = "Allows soft deleting projects" },
                new() { Name = "VIEW_ALL_PROJECTS",DisplayName = "View All Projects",Group = "Project", Description = "Allows viewing all projects" },
                new() { Name = "CREATE_PRODUCT",   DisplayName = "Create Product",   Group = "Product", Description = "Allows creating new products" },
                new() { Name = "EDIT_PRODUCT",     DisplayName = "Edit Product",     Group = "Product", Description = "Allows editing product details" },
                new() { Name = "MANAGE_RELEASE_NOTES", DisplayName = "Manage Release Notes", Group = "Product", Description = "Allows managing release notes" },
                new() { Name = "VIEW_PRODUCT_BACKLOG", DisplayName = "View Product Backlog", Group = "Product", Description = "Allows viewing product backlog" },
                new() { Name = "CREATE_TICKET",    DisplayName = "Create Ticket",    Group = "Ticket", Description = "Allows creating new tickets" },
                new() { Name = "EDIT_OWN_TICKETS", DisplayName = "Edit Own Tickets", Group = "Ticket", Description = "Allows editing own tickets" },
                new() { Name = "EDIT_ALL_TICKETS", DisplayName = "Edit All Tickets", Group = "Ticket", Description = "Allows editing all tickets" },
                new() { Name = "DELETE_TICKET",    DisplayName = "Delete Ticket",    Group = "Ticket", Description = "Allows deleting tickets" },
                new() { Name = "REASSIGN_TICKET",  DisplayName = "Reassign Ticket",  Group = "Ticket", Description = "Allows reassigning tickets" },
                new() { Name = "CHANGE_STATUS",    DisplayName = "Change Status",    Group = "Ticket", Description = "Allows changing ticket status" },
                new() { Name = "VIEW_ALL_TICKETS", DisplayName = "View All Tickets", Group = "Ticket", Description = "Allows viewing all tickets" },
                new() { Name = "APPROVE_TICKETS",  DisplayName = "Approve Tickets",  Group = "Ticket", Description = "Allows approving tickets" },
                new() { Name = "MANAGE_BRDS",         DisplayName = "Manage BRDs",          Group = "Backlog", Description = "Allows managing BRDs" },
                new() { Name = "MANAGE_USER_STORIES", DisplayName = "Manage User Stories",  Group = "Backlog", Description = "Allows managing user stories" },
                new() { Name = "APPROVE_REQUIREMENTS",DisplayName = "Approve Requirements", Group = "Backlog", Description = "Allows approving requirements" },
                new() { Name = "CREATE_SPRINT",        DisplayName = "Create Sprint",         Group = "Sprint", Description = "Allows creating sprints" },
                new() { Name = "CLOSE_SPRINT",         DisplayName = "Close Sprint",          Group = "Sprint", Description = "Allows closing sprints" },
                new() { Name = "MOVE_TICKETS_TO_SPRINT",DisplayName = "Move Tickets to Sprint",Group = "Sprint", Description = "Allows moving tickets to sprints" },
                new() { Name = "VIEW_REPORTS",     DisplayName = "View Reports",     Group = "Report", Description = "Allows viewing reports" },
                new() { Name = "EXPORT_REPORTS",   DisplayName = "Export Reports",   Group = "Report", Description = "Allows exporting reports" },
                new() { Name = "VIEW_COSTING_DATA",DisplayName = "View Costing Data",Group = "Report", Description = "Allows viewing costing data" },
                new() { Name = "VIEW_BUDGET_DATA", DisplayName = "View Budget Data", Group = "Report", Description = "Allows viewing budget data" },
                new() { Name = "MANAGE_USERS",                 DisplayName = "Manage Users",                  Group = "Settings", Description = "Allows managing users" },
                new() { Name = "MANAGE_ROLES",                 DisplayName = "Manage Roles",                  Group = "Settings", Description = "Allows managing roles" },
                new() { Name = "MANAGE_NOTIFICATION_SETTINGS", DisplayName = "Manage Notification Settings", Group = "Settings", Description = "Allows managing notification settings" },
                new() { Name = "MANAGE_ACCESS_RULES",          DisplayName = "Manage Access Rules",           Group = "Settings", Description = "Allows managing access rules" },
            };
            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            // Helper to get permission IDs by name
            Permission P(string name) => permissions.First(p => p.Name == name);

            // Seed 5 default roles with appropriate permissions
            var roles = new List<Role>
            {
                new Role
                {
                    Name = "Administrator", IsSystem = true, IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Full system access",
                    RolePermissions = permissions
                        .Select(p => new RolePermission { Permission = p })
                        .ToList() // Administrator gets all permissions
                },
                new Role
                {
                    Name = "Project Manager", IsSystem = true, IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Full project management access",
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = P("CREATE_PROJECT") },
                        new() { Permission = P("EDIT_PROJECT") },
                        new() { Permission = P("ARCHIVE_PROJECT") },
                        new() { Permission = P("VIEW_ALL_PROJECTS") },
                        new() { Permission = P("CREATE_PRODUCT") },
                        new() { Permission = P("EDIT_PRODUCT") },
                        new() { Permission = P("MANAGE_RELEASE_NOTES") },
                        new() { Permission = P("VIEW_PRODUCT_BACKLOG") },
                        new() { Permission = P("CREATE_TICKET") },
                        new() { Permission = P("EDIT_ALL_TICKETS") },
                        new() { Permission = P("REASSIGN_TICKET") },
                        new() { Permission = P("CHANGE_STATUS") },
                        new() { Permission = P("VIEW_ALL_TICKETS") },
                        new() { Permission = P("APPROVE_TICKETS") },
                        new() { Permission = P("MANAGE_BRDS") },
                        new() { Permission = P("APPROVE_REQUIREMENTS") },
                        new() { Permission = P("CREATE_SPRINT") },
                        new() { Permission = P("CLOSE_SPRINT") },
                        new() { Permission = P("MOVE_TICKETS_TO_SPRINT") },
                        new() { Permission = P("VIEW_REPORTS") },
                        new() { Permission = P("EXPORT_REPORTS") },
                        new() { Permission = P("VIEW_COSTING_DATA") },
                        new() { Permission = P("VIEW_BUDGET_DATA") },
                        new() { Permission = P("MANAGE_USERS") },
                        new() { Permission = P("MANAGE_NOTIFICATION_SETTINGS") },
                    }
                },
                new Role
                {
                    Name = "Developer", IsSystem = true, IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Regular development access",
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = P("VIEW_ALL_PROJECTS") },
                        new() { Permission = P("VIEW_PRODUCT_BACKLOG") },
                        new() { Permission = P("CREATE_TICKET") },
                        new() { Permission = P("EDIT_OWN_TICKETS") },
                        new() { Permission = P("CHANGE_STATUS") },
                        new() { Permission = P("VIEW_ALL_TICKETS") },
                        new() { Permission = P("MOVE_TICKETS_TO_SPRINT") },
                        new() { Permission = P("VIEW_REPORTS") },
                    }
                },
                new Role
                {
                    Name = "QA Engineer", IsSystem = true, IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Quality assurance access",
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = P("VIEW_ALL_PROJECTS") },
                        new() { Permission = P("VIEW_PRODUCT_BACKLOG") },
                        new() { Permission = P("CREATE_TICKET") },
                        new() { Permission = P("EDIT_OWN_TICKETS") },
                        new() { Permission = P("CHANGE_STATUS") },
                        new() { Permission = P("VIEW_ALL_TICKETS") },
                        new() { Permission = P("APPROVE_TICKETS") },
                        new() { Permission = P("VIEW_REPORTS") },
                    }
                },
                new Role
                {
                    Name = "Business Analyst", IsSystem = true, IsDefault = false,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Requirements management access",
                    RolePermissions = new List<RolePermission>
                    {
                        new() { Permission = P("VIEW_ALL_PROJECTS") },
                        new() { Permission = P("VIEW_PRODUCT_BACKLOG") },
                        new() { Permission = P("CREATE_TICKET") },
                        new() { Permission = P("EDIT_OWN_TICKETS") },
                        new() { Permission = P("VIEW_ALL_TICKETS") },
                        new() { Permission = P("MANAGE_BRDS") },
                        new() { Permission = P("MANAGE_USER_STORIES") },
                        new() { Permission = P("APPROVE_REQUIREMENTS") },
                        new() { Permission = P("VIEW_REPORTS") },
                    }
                }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        public static async Task SeedTicketStatusesAsync(ApplicationDbContext context, int projectId)
        {
            if (await context.TicketStatuses.AnyAsync(s => s.ProjectId == projectId)) return;

            var statuses = new List<TicketStatus>
            {
                new() { ProjectId = projectId, Name = "Open", Colour = "#CCCCCC", Order = 1, IsDefault = true, IsTerminal = false },
                new() { ProjectId = projectId, Name = "NotStarted", Colour = "#AAAAAA", Order = 2, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "Implementing", Colour = "#BBBBBB", Order = 3, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "WIP", Colour = "#3399FF", Order = 4, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "Paused", Colour = "#FFCC00", Order = 5, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "InReview", Colour = "#9966FF", Order = 6, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "QA", Colour = "#FF9900", Order = 7, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "UAT", Colour = "#00CC99", Order = 8, IsDefault = false, IsTerminal = false },
                new() { ProjectId = projectId, Name = "Completed", Colour = "#33CC33", Order = 9, IsDefault = false, IsTerminal = true },
                new() { ProjectId = projectId, Name = "Closed", Colour = "#666666", Order = 10, IsDefault = false, IsTerminal = true },
                new() { ProjectId = projectId, Name = "Cancelled", Colour = "#FF3300", Order = 11, IsDefault = false, IsTerminal = true },
                new() { ProjectId = projectId, Name = "Reopened", Colour = "#FFCCFF", Order = 12, IsDefault = false, IsTerminal = false }
            };

            await context.TicketStatuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }
    }
}
