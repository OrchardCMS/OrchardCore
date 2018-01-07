using System;
using System.Linq;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ScriptTaskDisplay : ActivityDisplayDriver<ScriptTask, ScriptTaskViewModel>
    {
        protected override void Map(ScriptTask source, ScriptTaskViewModel target)
        {
            target.Title = source.Title;
            target.AvailableOutcomes = string.Join(", ", source.AvailableOutcomes);
            target.Script = source.Script;
        }

        protected override void Map(ScriptTaskViewModel source, ScriptTask target)
        {
            target.Title = source.Title?.Trim();
            target.AvailableOutcomes = source.AvailableOutcomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            target.Script = source.Script;
        }
    }
}
