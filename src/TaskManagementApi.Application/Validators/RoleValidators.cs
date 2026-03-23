using FluentValidation;
using TaskManagementApi.Application.DTOs.Roles;

namespace TaskManagementApi.Application.Validators
{
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters");
            RuleFor(x => x.ParentRoleId)
                .GreaterThan(0).When(x => x.ParentRoleId.HasValue)
                .WithMessage("Parent role ID must be valid");
        }
    }

    public class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
    {
        public AssignRoleRequestValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.ProjectId).GreaterThan(0);
            RuleFor(x => x.RoleId).GreaterThan(0);
        }
    }
}
