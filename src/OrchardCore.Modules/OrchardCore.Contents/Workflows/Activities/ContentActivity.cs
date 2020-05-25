using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentActivity : Activity
    {
        public const string ContentItemInputKey = ContentEventConstants.ContentItemInputKey;

        protected readonly IStringLocalizer S;

        protected ContentActivity(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer)
        {
            ContentManager = contentManager;
            ScriptEvaluator = scriptEvaluator;
            S = localizer;
        }

        protected IContentManager ContentManager { get; }

        protected IWorkflowScriptEvaluator ScriptEvaluator { get; }

        /// <summary>
        /// Whether the activity is executed inline from a content event.
        /// </summary>
        protected bool FromContentEvent { get; private set; }

        /// <summary>
        /// The original correlation id when the workflow is started or resumed.
        /// </summary>
        protected string OriginalCorrelationId { get; private set; }

        /// <summary>
        /// Whether the activity is executed inline from a correlated content event.
        /// </summary>
        protected bool InlineFromContentEvent { get; private set; }

        public override LocalizedString Category => S["Content"];

        /// <summary>
        /// An expression that evaluates to an <see cref="IContent"/> item.
        /// </summary>
        public WorkflowExpression<IContent> Content
        {
            get => GetProperty(() => new WorkflowExpression<IContent>());
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public override Task OnInputReceivedAsync(WorkflowExecutionContext workflowContext, IDictionary<string, object> input)
        {
            if (input?.GetValue<string>(ContentEventConstants.FromContentEvent) != null)
            {
                FromContentEvent = true;
            }

            OriginalCorrelationId = workflowContext.CorrelationId;

            return Task.CompletedTask;
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }

        protected virtual async Task<IContent> GetContentAsync(WorkflowExecutionContext workflowContext)
        {
            IContent content;

            // Try to evaluate a content item from the Content expression, if provided.
            if (!string.IsNullOrWhiteSpace(Content.Expression))
            {
                var expression = new WorkflowExpression<object> { Expression = Content.Expression };
                var result = await ScriptEvaluator.EvaluateAsync(expression, workflowContext);

                if (result is ContentItem contentItem)
                {
                    content = contentItem;
                }
                else if (result is string contentItemId)
                {
                    content = new ContentItemIdExpressionContent() { ContentItem = new ContentItem() { ContentItemId = contentItemId } };
                }
                else
                {
                    // Try to map the result to a content item.
                    var json = JsonConvert.SerializeObject(result);
                    content = JsonConvert.DeserializeObject<ContentItem>(json);
                }
            }
            else
            {
                // If no expression was provided, see if the content item was provided as an input or as a property.
                content = workflowContext.Input.GetValue<IContent>(ContentItemInputKey)
                    ?? workflowContext.Properties.GetValue<IContent>(ContentItemInputKey);
            }

            if (content != null && content.ContentItem.ContentItemId != null)
            {
                UpdateInlineFromContentEventProperty(content.ContentItem.ContentItemId);

                return content;
            }

            return null;
        }

        protected virtual async Task<string> GetContentItemIdAsync(WorkflowExecutionContext workflowContext)
        {
            // Try to evaluate a content item id from the Content expression, if provided.
            if (!string.IsNullOrWhiteSpace(Content.Expression))
            {
                var expression = new WorkflowExpression<object> { Expression = Content.Expression };
                var contentItemIdResult = await ScriptEvaluator.EvaluateAsync(expression, workflowContext);

                if (contentItemIdResult is string contentItemId)
                {
                    UpdateInlineFromContentEventProperty(contentItemId);

                    return contentItemId;
                }
            }

            return null;
        }

        protected void UpdateInlineFromContentEventProperty(string contentItemId)
        {
            InlineFromContentEvent = FromContentEvent && String.Equals(OriginalCorrelationId, contentItemId, StringComparison.OrdinalIgnoreCase);
        }

        protected class ContentItemIdExpressionContent : IContent
        {
            public ContentItem ContentItem { get; set; }
        }
    }
}
