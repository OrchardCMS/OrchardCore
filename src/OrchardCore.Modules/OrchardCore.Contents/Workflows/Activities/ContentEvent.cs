using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentEvent : EventActivity
    {
        protected readonly IStringLocalizer S;

        public ContentEvent(IStringLocalizer s)
        {
            S = s;
        }

        public override bool CanStartWorkflow => true;
        public override LocalizedString Category => S["Content Items"];

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            try
            {
                var contentTypesState = this.As<string>("ContentTypes");

                // "" means 'any'.
                if (string.IsNullOrEmpty(contentTypesState))
                {
                    return true;
                }

                var contentTypes = contentTypesState.Split(',');
                var content = GetContent(workflowContext);

                if (content == null)
                {
                    return false;
                }

                return contentTypes.Any(contentType => content.ContentItem.ContentType == contentType);
            }
            catch
            {
                return false;
            }
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