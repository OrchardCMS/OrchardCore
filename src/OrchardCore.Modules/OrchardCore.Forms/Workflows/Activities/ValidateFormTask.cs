using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    public class ValidateFormTask : TaskActivity
    {
        private readonly IUpdateModelAccessor _updateModelAccessor;
        protected readonly IStringLocalizer S;

        public ValidateFormTask(
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<ValidateFormTask> localizer
        )
        {
            _updateModelAccessor = updateModelAccessor;
            S = localizer;
        }

        public override string Name => nameof(ValidateFormTask);

        public override LocalizedString DisplayText => S["Validate Form Task"];

        public override LocalizedString Category => S["Validation"];

        public override bool HasEditor => false;

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Valid"], S["Invalid"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var updater = _updateModelAccessor.ModelUpdater
                ?? throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");

            var isValid = updater.ModelState.ErrorCount == 0;
            var outcome = isValid ? "Valid" : "Invalid";
            return Outcomes(outcome);
        }
    }
}
