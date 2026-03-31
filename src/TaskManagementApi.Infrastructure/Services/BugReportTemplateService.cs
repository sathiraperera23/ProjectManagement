using System.Text;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Infrastructure.Services
{
    public class BugReportTemplateService : IBugReportTemplateService
    {
        private readonly IRepository<Project> _projectRepository;

        public BugReportTemplateService(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<string> GetTemplateAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new KeyNotFoundException($"Project {projectId} not found");

            var sb = new StringBuilder();
            sb.AppendLine($"TO: {project.IntakeEmailAddress}");
            sb.AppendLine("SUBJECT: [Bug Report] Brief description of the issue");
            sb.AppendLine();
            sb.AppendLine("==============================");
            sb.AppendLine("BUG REPORT — Please fill in all fields below.");
            sb.AppendLine("Do not modify the field labels.");
            sb.AppendLine("==============================");
            sb.AppendLine();
            sb.AppendLine("TITLE:");
            sb.AppendLine("[Write a short summary of the bug here]");
            sb.AppendLine();
            sb.AppendLine("DESCRIPTION:");
            sb.AppendLine("[Describe the bug in detail]");
            sb.AppendLine();
            sb.AppendLine("STEPS TO REPRODUCE:");
            sb.AppendLine("1.");
            sb.AppendLine("2.");
            sb.AppendLine("3.");
            sb.AppendLine();
            sb.AppendLine("EXPECTED BEHAVIOUR:");
            sb.AppendLine("[What should have happened]");
            sb.AppendLine();
            sb.AppendLine("ACTUAL BEHAVIOUR:");
            sb.AppendLine("[What actually happened]");
            sb.AppendLine();
            sb.AppendLine("ENVIRONMENT:");
            sb.AppendLine("[Browser / Device / App version / OS]");
            sb.AppendLine();
            sb.AppendLine("SEVERITY:");
            sb.AppendLine("[Critical / Major / Minor / Trivial]");
            sb.AppendLine();
            sb.AppendLine("==============================");
            sb.AppendLine("Attachments: You may attach screenshots or files to this email.");
            sb.AppendLine("Replies to this email will be appended to your ticket.");

            if (!string.IsNullOrWhiteSpace(project.BugReportTemplateCustomText))
            {
                sb.AppendLine(project.BugReportTemplateCustomText);
            }

            sb.AppendLine("==============================");

            return sb.ToString();
        }

        public async Task UpdateCustomTextAsync(int projectId, string customText, int updatedByUserId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new KeyNotFoundException($"Project {projectId} not found");

            project.BugReportTemplateCustomText = customText;
            await _projectRepository.UpdateAsync(project);
        }
    }
}
