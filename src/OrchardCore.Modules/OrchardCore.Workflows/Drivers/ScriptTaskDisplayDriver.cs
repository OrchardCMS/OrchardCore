using System;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ScriptTaskDisplayDriver : ActivityDisplayDriver<ScriptTask, ScriptTaskViewModel>
    {
        protected override void EditActivity(ScriptTask source, ScriptTaskViewModel model)
        {
            model.AvailableOutcomes = string.Join(", ", source.AvailableOutcomes);
            model.Script = source.Script.Expression;
        }

        protected override void UpdateActivity(ScriptTaskViewModel model, ScriptTask activity)
        {
            activity.AvailableOutcomes = model.AvailableOutcomes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            activity.Script = new WorkflowExpression<object>(model.Script);
        }
    }
}
