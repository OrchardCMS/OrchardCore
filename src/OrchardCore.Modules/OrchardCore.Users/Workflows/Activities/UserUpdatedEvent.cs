using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public class UserUpdatedEvent : UserEvent
    {
        public UserUpdatedEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<UserUpdatedEvent> localizer) : base(userService, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(UserUpdatedEvent);

        public override LocalizedString DisplayText => S["User Updated Event"];
    }
}
