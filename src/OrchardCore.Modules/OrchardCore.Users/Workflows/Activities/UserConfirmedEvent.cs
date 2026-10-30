using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserConfirmedEvent : UserEvent, IActivityDescription
{
    public UserConfirmedEvent(
        IUserService userService,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<UserConfirmedEvent> stringLocalizer)
        : base(userService, scriptEvaluator, localizerFactory, stringLocalizer)
    {
    }

    public override string Name
        => nameof(UserConfirmedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserConfirmedEvent>("User Confirmed Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
