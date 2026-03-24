using FluentValidation;
using TaskManagementApi.Application.DTOs.Backlog;
using TaskManagementApi.Domain.Enums;

namespace TaskManagementApi.Application.Validators
{
    public class CreateBacklogItemRequestValidator : AbstractValidator<CreateBacklogItemRequest>
    {
        public CreateBacklogItemRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Priority).IsInEnum();
            RuleFor(x => x).Must(x => x.ProjectId.HasValue || x.ProductId.HasValue);
            RuleFor(x => x.AcceptanceCriteria).NotEmpty().When(x => x.Type == BacklogItemType.UserStory);
        }
    }

    public class UpdateBacklogItemRequestValidator : AbstractValidator<UpdateBacklogItemRequest>
    {
        public UpdateBacklogItemRequestValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.ChangeNote).NotEmpty();
        }
    }
}
