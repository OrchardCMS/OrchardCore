using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities
{
    public class ScriptTask : TaskActivity
    {
        public ScriptTask(IStringLocalizer<ScriptTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override string Name => nameof(ScriptTask);
        public override LocalizedString Category => T["Control Flow"];
        public override LocalizedString Description => T["Executes the specified script and continues execution based on any outcomes it sets."];

        public IList<string> AvailableOutcomes
        {
            get => GetProperty(() => new List<string> { "Done" });
            set => SetProperty(value);
        }

        /// <summary>
        /// A title describing the work done by the script.
        /// </summary>
        public string Title
        {
            get => GetProperty<String>();
            set => SetProperty(value);
        }

        /// <summary>
        /// The script can call any available functions, including setOutcome().
        /// </summary>
        public string Script
        {
            get => GetProperty(() => "setOutcome('Done');");
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(AvailableOutcomes.Select(x => T[x]).ToArray());
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var outcomes = new List<string>();
            workflowContext.Evaluate(Script, new OutcomeMethodProvider(outcomes));
            return outcomes;
        }
    }
}