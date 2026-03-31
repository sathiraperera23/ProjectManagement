using FluentValidation;
using TaskManagementApi.Application.DTOs.Notifications;

namespace TaskManagementApi.Application.Validators
{
    public class UpdateNotificationPreferenceRequestValidator : AbstractValidator<UpdateNotificationPreferenceRequest>
    {
        public UpdateNotificationPreferenceRequestValidator()
        {
            RuleForEach(x => x.Preferences).SetValidator(new NotificationPreferenceDtoValidator());
        }
    }

    public class NotificationPreferenceDtoValidator : AbstractValidator<NotificationPreferenceDto>
    {
        public NotificationPreferenceDtoValidator()
        {
            RuleFor(x => x.EventType).IsInEnum();
        }
    }
}
