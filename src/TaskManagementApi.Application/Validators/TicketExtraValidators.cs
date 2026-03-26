using FluentValidation;
using TaskManagementApi.Application.DTOs.Tickets;

namespace TaskManagementApi.Application.Validators
{
    public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
    {
        public CreateCommentRequestValidator()
        {
            RuleFor(x => x.Body).NotEmpty();
        }
    }

    public class CreateDailyUpdateRequestValidator : AbstractValidator<CreateDailyUpdateRequest>
    {
        public CreateDailyUpdateRequestValidator()
        {
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.WorkedOn).NotEmpty();
            RuleFor(x => x.PlannedNext).NotEmpty();
        }
    }
}
