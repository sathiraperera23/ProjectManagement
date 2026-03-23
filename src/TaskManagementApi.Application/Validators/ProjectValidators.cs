using FluentValidation;
using TaskManagementApi.Application.DTOs.Projects;

namespace TaskManagementApi.Application.Validators
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ProjectCode).MaximumLength(10);
            RuleFor(x => x.StartDate).NotEmpty();
        }
    }

    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ProjectCode).MaximumLength(10);
            RuleFor(x => x.StartDate).NotEmpty();
        }
    }
}
