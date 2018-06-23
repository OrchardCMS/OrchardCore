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

        public AddModelValidationErrorTask(
            IUpdateModelAccessor updateModelAccessor,
            IStringLocalizer<AddModelValidationErrorTask> localizer
        )
        {
            _updateModelAccessor = updateModelAccessor;
            T = localizer;
        }

        public override string Name => nameof(AddModelValidationErrorTask);
        public override LocalizedString Category => T["Validation"];

        private IStringLocalizer T { get; set; }

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
            return Outcomes(T["Done"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var updater = _updateModelAccessor.ModelUpdater;

            if (updater == null)
            {
                throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");
            }

            updater.ModelState.AddModelError(Key, ErrorMessage);
            return Outcomes("Done");
        }
    }
}