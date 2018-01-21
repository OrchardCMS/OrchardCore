using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class LogTaskDisplay : ActivityDisplayDriver<LogTask, LogTaskViewModel>
    {
        protected override void Map(LogTask source, LogTaskViewModel target)
        {
            target.LogLevel = source.LogLevel;
            target.Text = source.Text.Expression;
        }

        protected override void Map(LogTaskViewModel source, LogTask target)
        {
            target.LogLevel = source.LogLevel;
            target.Text = new WorkflowExpression<string>(source.Text);
        }
    }
}
