using System;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ScriptTaskDisplay : ActivityDisplayDriver<ScriptTask, ScriptTaskViewModel>
    {
        protected override void EditActivity(ScriptTask source, ScriptTaskViewModel model)
        {
            model.Title = source.Title;
            model.AvailableOutcomes = string.Join(", ", source.AvailableOutcomes);
            model.Script = source.Script.Expression;
        }

        protected override void UpdateActivity(ScriptTaskViewModel model, ScriptTask activity)
        {
            activity.Title = model.Title?.Trim()?? string.Empty;
            activity.AvailableOutcomes = model.AvailableOutcomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            activity.Script = new WorkflowExpression<object>(model.Script);
        }
    }
}
