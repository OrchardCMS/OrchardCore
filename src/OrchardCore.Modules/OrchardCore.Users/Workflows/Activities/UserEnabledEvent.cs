using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserEnabledEvent : UserEvent, IActivityDescription
{
    public UserEnabledEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<UserEnabledEvent> localizer) : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserEnabledEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserEnabledEvent>("User Enabled Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
