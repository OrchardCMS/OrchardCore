using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentEvent : EventActivity
    {
        protected ContentEvent(IContentManager contentManager, IStringLocalizer s)
        {
            ContentManager = contentManager;
            S = s;
        }

        protected IContentManager ContentManager { get; }
        protected IStringLocalizer S { get; }
        public override bool CanStartWorkflow => true;
        public override LocalizedString Category => S["Content"];

        public IList<string> ContentTypeFilter
        {
            get => GetProperty<IList<string>>(defaultValue: () => new List<string>());
            set => SetProperty(value);
        }

        /// <summary>
        /// An expression that evaluates to either an <see cref="IContent"/> item or a content item ID.
        /// </summary>
        public WorkflowExpression<object> ContentExpression
        {
            get => GetProperty<WorkflowExpression<object>>();
            set => SetProperty(value);
        }

        public override async Task<bool> CanExecuteAsync(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            var content = await GetContentAsync(workflowContext);

            if (content == null)
            {
                return false;
            }

            var contentTypes = ContentTypeFilter.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            // "" means 'any'.
            return !contentTypes.Any() || contentTypes.Any(contentType => content.ContentItem.ContentType == contentType);
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

            var contentItem = contentValue as IContent;

            if (contentItem == null)
            {
                // Perhaps the expression returned a content ID?
                var contentId = contentValue as string;

                if (contentId != null)
                {
                    contentItem = await ContentManager.GetAsync(contentId);
                }
            }

            return contentItem;
        }
    }
}