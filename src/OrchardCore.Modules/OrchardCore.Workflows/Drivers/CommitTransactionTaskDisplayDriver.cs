using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class CommitTransactionTaskDisplayDriver : ActivityDisplayDriver<CommitTransactionTask, CommitTransactionTaskViewModel>
    {
        protected override void EditActivity(CommitTransactionTask activity, CommitTransactionTaskViewModel model)
        {
        }

        protected override void UpdateActivity(CommitTransactionTaskViewModel model, CommitTransactionTask activity)
        {
        }
    }
}
