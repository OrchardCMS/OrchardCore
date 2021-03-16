using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using YesSql;

namespace OrchardCore.Workflows.Activities
{
    public class CommitTransactionTask : TaskActivity
    {
        private readonly IStringLocalizer S;

        public CommitTransactionTask(IStringLocalizer<CommitTransactionTask> localizer)
        {
            S = localizer;
        }

        public override string Name => nameof(CommitTransactionTask);

        public override LocalizedString DisplayText => S["Commit Transaction Task"];

        public override LocalizedString Category => S["Primitives"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            await ShellScope.Services.GetRequiredService<ISession>().CommitAsync();
            return Outcomes("Done");
        }
    }
}
