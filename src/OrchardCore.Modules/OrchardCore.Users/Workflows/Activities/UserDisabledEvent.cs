using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserDisabledEvent : UserEvent, IActivityDescription
{
    public UserDisabledEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<UserDisabledEvent> localizer) : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserDisabledEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserDisabledEvent>("User Disabled Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
