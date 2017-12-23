using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentTask : Activity
    {
        protected readonly IStringLocalizer S;

        public ContentTask(IStringLocalizer s)
        {
            S = s;
        }

        public override LocalizedString Category => S["Content Items"];

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = GetContent(workflowContext);
            return content != null;
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return "Done";
        }

        protected IContent GetContent(WorkflowContext workflowContext)
        {
            return workflowContext.WorkflowInstance.State.As<IContent>();
        }
    }
}