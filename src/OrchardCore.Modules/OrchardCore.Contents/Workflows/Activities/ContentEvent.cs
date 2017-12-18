using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
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
                var contentTypesState = activityContext.Activity.As<string>("ContentTypes");

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