using FluentValidation;
using TaskManagementApi.Application.DTOs.SubProjects;

namespace TaskManagementApi.Application.Validators
{
    public class CreateSubProjectRequestValidator : AbstractValidator<CreateSubProjectRequest>
    {
        public CreateSubProjectRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }

    public class UpdateSubProjectRequestValidator : AbstractValidator<UpdateSubProjectRequest>
    {
        public UpdateSubProjectRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }
    }
}
