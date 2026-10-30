using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserLoggedInEvent : UserEvent, IActivityDescription
{
    public UserLoggedInEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<UserLoggedInEvent> localizer) : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserLoggedInEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserLoggedInEvent>("User Loggedin Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);

    public override IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "UserName", TypeName = "string", Description = LocalizationSource.Create<UserLoggedInEvent>("The name of the user who logged in.") },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Roles", TypeName = "array", Description = LocalizationSource.Create<UserLoggedInEvent>("The names of the user's roles.") },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Provider", TypeName = "string", Description = LocalizationSource.Create<UserLoggedInEvent>("The external login provider, or nothing for a local login.") },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "ExternalClaims", TypeName = "array", Description = LocalizationSource.Create<UserLoggedInEvent>("The claims of the external login.") },
        ];
}
