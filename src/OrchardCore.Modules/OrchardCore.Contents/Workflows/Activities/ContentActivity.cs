using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentActivity : Activity
    {
        protected ContentActivity(IContentManager contentManager, IStringLocalizer localizer)
        {
            ContentManager = contentManager;
            T = localizer;
        }

        protected IContentManager ContentManager { get; }
        protected IStringLocalizer T { get; }
        public override LocalizedString Category => T["Content"];

        public IList<string> ContentTypeFilter
        {
            get => GetProperty<IList<string>>(defaultValue: () => new List<string>());
            set => SetProperty(value);
        }

        /// <summary>
        /// An expression that evaluates to either a <see cref="IContent"/> item.
        /// </summary>
        public WorkflowExpression<IContent> Content
        {
            get => GetProperty(() => new WorkflowExpression<IContent>());
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
            return Outcomes(T["Done"]);
        }

        public override IEnumerable<string> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return "Done";
        }

        protected virtual async Task<IContent> GetContentAsync(WorkflowContext workflowContext)
        {
            // Try and evaluate a content item from the Content expression, if provided.
            if (!string.IsNullOrWhiteSpace(Content.Expression))
            {
                return await workflowContext.EvaluateScriptAsync(Content);
            }

            // If no expression was provided, see if the content item was provided as an input using the "Content" key.
            var content = workflowContext.Input.GetValue<IContent>("Content");

            if (content != null)
            {
                return content;
            }

            return null;
        }
    }
}