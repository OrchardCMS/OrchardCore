using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserConfirmedEvent : UserEvent
{
    public UserConfirmedEvent(
        IUserService userService,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizer<UserUpdatedEvent> stringLocalizer)
        : base(userService, scriptEvaluator, stringLocalizer)
    {
    }

    public override string Name
        => nameof(UserConfirmedEvent);

    public override LocalizedString DisplayText
        => S["User Confirmed Event"];
}
