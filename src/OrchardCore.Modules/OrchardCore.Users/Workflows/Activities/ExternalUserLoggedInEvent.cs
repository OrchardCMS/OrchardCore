using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Users.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Workflows.Activities
{
    public class ExternalUserLoggedInEvent : UserEvent
    {
        public ExternalUserLoggedInEvent(IUserService userService, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ExternalUserLoggedInEvent> localizer) : base(userService, scriptEvaluator, localizer)
        {
        }

        public override string Name => nameof(ExternalUserLoggedInEvent);
        public override LocalizedString DisplayText => T["External User Loggedin Event"];
    }
}