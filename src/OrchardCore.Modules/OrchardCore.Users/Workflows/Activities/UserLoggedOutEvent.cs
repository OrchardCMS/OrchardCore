using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserLoggedOutEvent : UserEvent, IActivityDescription
{
    public UserLoggedOutEvent(
        IUserService userService,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<UserLoggedOutEvent> localizer)
        : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserLoggedOutEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserLoggedOutEvent>("User Loggedout Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);

    public override IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "UserName", TypeName = "string", Description = LocalizationSource.Create<UserLoggedOutEvent>("The name of the user who logged out.") },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "Roles", TypeName = "array", Description = LocalizationSource.Create<UserLoggedOutEvent>("The names of the user's roles.") },
        ];
}
