using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class MissingActivity : Activity
    {
        private readonly ILogger<MissingActivity> _logger;

        public MissingActivity(IStringLocalizer<MissingActivity> localizer, ILogger<MissingActivity> logger, ActivityRecord missingActivityRecord)
        {
            T = localizer;
            _logger = logger;
            MissingActivityRecord = missingActivityRecord;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(MissingActivity);
        public override LocalizedString Category => T["Exceptions"];
        public override LocalizedString Description => T["A replacement when a referenced activity could not be found."];

        public ActivityRecord MissingActivityRecord { get; }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            yield break;
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _logger.LogWarning($"Activity {MissingActivityRecord.Name} is no longer available. This can happen if the feature providing the activity is no longer enabled. Either enable the feature, or remove this activity from workflow definition with ID {workflowContext.WorkflowDefinition.Id}");
            return Noop();
        }
    }
}