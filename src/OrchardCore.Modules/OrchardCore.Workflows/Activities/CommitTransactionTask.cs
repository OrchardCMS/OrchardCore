using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Activities
{
    public class CommitTransactionTask : TaskActivity
    {
        private readonly ISession _session;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        protected readonly IStringLocalizer S;

        public CommitTransactionTask(
            ISession session,
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<CommitTransactionTask> localizer)
        {
            _session = session;
            _updateModelAccessor = updateModelAccessor;
            S = localizer;
        }

        public override string Name => nameof(CommitTransactionTask);

        public override LocalizedString DisplayText => S["Commit Transaction Task"];

        public override LocalizedString Category => S["Session"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Valid"], S["Invalid"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (!_updateModelAccessor.ModelUpdater.ModelState.IsValid)
            {
                return Outcomes("Done", "Invalid");
            }

            await _session.SaveChangesAsync();
            return Outcomes("Done", "Valid");
        }
    }
}
