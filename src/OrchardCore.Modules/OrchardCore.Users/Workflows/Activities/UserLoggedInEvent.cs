using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserLoggedInEvent : UserEvent
{
    public UserLoggedInEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<UserLoggedInEvent> localizer) : base(userService, scriptEvaluator, localizer)
    {
    }

    public override string Name => nameof(UserLoggedInEvent);

    public override LocalizedString DisplayText => S["User Loggedin Event"];

    public override IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "UserName", TypeName = "string", Description = S["The name of the user who logged in."] },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Roles", TypeName = "array", Description = S["The names of the user's roles."] },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Provider", TypeName = "string", Description = S["The external login provider, or nothing for a local login."] },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "ExternalClaims", TypeName = "array", Description = S["The claims of the external login."] },
        ];
}
