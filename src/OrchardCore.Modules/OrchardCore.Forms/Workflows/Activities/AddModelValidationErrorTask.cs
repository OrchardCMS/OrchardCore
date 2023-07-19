using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities
{
    public class AddModelValidationErrorTask : TaskActivity
    {
        private readonly IUpdateModelAccessor _updateModelAccessor;
        protected readonly IStringLocalizer S;

        public AddModelValidationErrorTask(
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<AddModelValidationErrorTask> localizer
        )
        {
            _updateModelAccessor = updateModelAccessor;
            S = localizer;
        }

        public override string Name => nameof(AddModelValidationErrorTask);

        public override LocalizedString DisplayText => S["Add Model Validation Error Task"];

        public override LocalizedString Category => S["Validation"];

        public string Key
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string ErrorMessage
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var updater = _updateModelAccessor.ModelUpdater
                ?? throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");

            updater.ModelState.AddModelError(Key, ErrorMessage);
            return Outcomes("Done");
        }
    }
}
