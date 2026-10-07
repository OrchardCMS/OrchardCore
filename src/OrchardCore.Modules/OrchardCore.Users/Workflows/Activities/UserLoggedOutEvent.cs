using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
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

    public override IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "UserName", TypeName = "string", Description = S["The name of the user who logged out."] },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Roles", TypeName = "array", Description = S["The names of the user's roles."] },
        ];
}
