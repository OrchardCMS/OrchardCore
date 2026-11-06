using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserCreatedEvent : UserEvent, IActivityDescription
{
    public UserCreatedEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<UserCreatedEvent> localizer) : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserCreatedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserCreatedEvent>("User Created Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
