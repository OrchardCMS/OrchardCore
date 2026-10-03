using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserLoggedOutEvent : UserEvent
{
    public UserLoggedOutEvent(
        IUserService userService,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizer<UserLoggedOutEvent> localizer)
        : base(userService, scriptEvaluator, localizer)
    {
    }

    public override string Name => nameof(UserLoggedOutEvent);

    public override LocalizedString DisplayText => S["User Loggedout Event"];
}
