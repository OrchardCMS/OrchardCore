using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities;

public class UserUpdatedEvent : UserEvent, IActivityDescription
{
    public UserUpdatedEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<UserUpdatedEvent> localizer) : base(userService, scriptEvaluator, localizerFactory, localizer)
    {
    }

    public override string Name => nameof(UserUpdatedEvent);

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserUpdatedEvent>("User Updated Event");

    public override LocalizedString DisplayText => LocalizerFactory.Localize(DisplayName);
}
