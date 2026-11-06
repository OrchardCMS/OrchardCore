using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserDeletedEvent : UserEvent, IActivityDescription
{
    public UserDeletedEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<UserDeletedEvent> localizer) : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserDeletedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserDeletedEvent>("User Deleted Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
