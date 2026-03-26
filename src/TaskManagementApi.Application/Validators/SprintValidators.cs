using FluentValidation;
using TaskManagementApi.Application.DTOs.Sprints;

namespace TaskManagementApi.Application.Validators
{
    public class CreateSprintRequestValidator : AbstractValidator<CreateSprintRequest>
    {
        public CreateSprintRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate);
            RuleFor(x => x.StoryPointCapacity).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateSprintRequestValidator : AbstractValidator<UpdateSprintRequest>
    {
        public UpdateSprintRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate);
            RuleFor(x => x.StoryPointCapacity).GreaterThanOrEqualTo(0);
        }
    }

    public class CloseSprintRequestValidator : AbstractValidator<CloseSprintRequest>
    {
        public CloseSprintRequestValidator()
        {
            RuleFor(x => x.Disposition).IsInEnum();
            RuleFor(x => x.NextSprintId).NotEmpty().When(x => x.Disposition == Domain.Enums.SprintClosureDisposition.MoveToNextSprint);
        }
    }
}
