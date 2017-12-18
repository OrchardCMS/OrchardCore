using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
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

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { S["Done"] };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return S["Done"];
        }

        protected IContent GetContent(WorkflowContext workflowContext)
        {
            return workflowContext.WorkflowInstance.State.As<IContent>();
        }
    }
}