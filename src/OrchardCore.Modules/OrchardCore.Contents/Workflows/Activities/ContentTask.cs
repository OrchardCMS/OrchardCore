using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentTask : Activity
    {
        public ContentTask(IContentManager contentManager, IStringLocalizer s)
        {
            ContentManager = contentManager;
            S = s;
        }

        protected IContentManager ContentManager { get; }
        protected IStringLocalizer S { get; }
        public override LocalizedString Category => S["Content"];

        /// <summary>
        /// An expression that evaluates to either an <see cref="IContent"/> item or a content item ID.
        /// </summary>
        public WorkflowExpression<object> ContentExpression
        {
            get => GetProperty(defaultValue: () => new WorkflowExpression<object>());
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetContentAsync(workflowContext);
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

        protected async Task<IContent> GetContentAsync(WorkflowContext workflowContext)
        {
            // Try and evaluate a content item from the ContentExpression, if provided.
            // If no expression was provided, assume the content item was provided as an input using the "Content" key.
            var contentValue = !String.IsNullOrWhiteSpace(ContentExpression.Expression)
                ? workflowContext.Evaluate(ContentExpression)
                : workflowContext.Input.GetValue("Content");

            var contentId = contentValue as string;

            return contentId != null ? await ContentManager.GetAsync(contentId, VersionOptions.Latest) : null;
        }
    }
}