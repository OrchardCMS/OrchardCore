using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class LogTaskDisplayDriver : ActivityDisplayDriver<LogTask, LogTaskViewModel>
    {
        protected override void EditActivity(LogTask activity, LogTaskViewModel model)
        {
            model.LogLevel = activity.LogLevel;
            model.Text = activity.Text.Expression;
        }

        protected override void UpdateActivity(LogTaskViewModel model, LogTask activity)
        {
            activity.LogLevel = model.LogLevel;
            activity.Text = new WorkflowExpression<string>(model.Text);
        }
    }
}
