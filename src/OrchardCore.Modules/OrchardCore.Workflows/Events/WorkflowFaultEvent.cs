using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Events
{
    public class WorkflowFaultEvent : EventActivity
    {
        protected readonly IStringLocalizer<WorkflowFaultEvent> S;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;

        public WorkflowFaultEvent(
            IStringLocalizer<WorkflowFaultEvent> stringLocalizer,
            IWorkflowScriptEvaluator scriptEvaluator)
        {
            S = stringLocalizer;
            _scriptEvaluator = scriptEvaluator;
        }

        public override string Name => nameof(WorkflowFaultEvent);
        public override LocalizedString DisplayText => S["Catch Workflow Fault Event"];
        public override LocalizedString Category => S["Background"];

        public WorkflowExpression<bool> ErrorFilter
        {
            get => GetProperty(() => new WorkflowExpression<bool>(GetDefaultValue()));
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }

        public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var faultModel = workflowContext.Input[WorkflowFaultModel.WorkflowFaultInputKey] as WorkflowFaultModel;

            // Avoid endless loops.
            if (faultModel == null || faultModel.WorkflowName == workflowContext.WorkflowType.Name)
            {
                return false;
            }

            return await _scriptEvaluator.EvaluateAsync(ErrorFilter, workflowContext);
        }

        private static string GetDefaultValue()
        {
            var sample = $@"//sample code
var errorInfo= input('{WorkflowFaultModel.WorkflowFaultInputKey}');
// This is where you define the workflow to intercept or specify the exception information
var result=  errorInfo.{nameof(WorkflowFaultModel.WorkflowName)}== 'WorkflowName' ||
errorInfo.{nameof(WorkflowFaultModel.WorkflowId)}== 'WorkflowId' ||
errorInfo.{nameof(WorkflowFaultModel.ErrorMessage)}.indexOf('ErrorStr') ||
errorInfo.{nameof(WorkflowFaultModel.ExceptionDetails)}.indexOf('ErrorStr') ||
errorInfo.{nameof(WorkflowFaultModel.FaultMessage)}.indexOf('ErrorStr') ||
errorInfo.{nameof(WorkflowFaultModel.ActivityDisplayName)}== 'ActivityDisplayName' ||
errorInfo.{nameof(WorkflowFaultModel.ActivityTypeName)}== 'ActivityTypeName' ||
errorInfo.{nameof(WorkflowFaultModel.ActivityId)}== 'ActivityId' ||
errorInfo.{nameof(WorkflowFaultModel.ExecutedActivityCount)}== 20
// If the above expression is true, the exception message will be caught
// and a new workflow instance will be created.
return result;";

            return sample;
        }
    }
}
