using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public abstract class ContentActivity : Activity
    {
        protected ContentActivity(IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer localizer)
        {
            ContentManager = contentManager;
            ScriptEvaluator = scriptEvaluator;
            T = localizer;
        }

        protected IContentManager ContentManager { get; }
        protected IWorkflowScriptEvaluator ScriptEvaluator { get; }
        protected IStringLocalizer T { get; }
        public override LocalizedString Category => T["Content"];

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
            return Outcomes(T["Done"]);
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes("Done");
        }

        protected virtual async Task<IContent> GetContentAsync(WorkflowExecutionContext workflowContext)
        {
            // Try and evaluate a content item from the Content expression, if provided.
            if (!string.IsNullOrWhiteSpace(Content.Expression))
            {
                var expression = new WorkflowExpression<object> { Expression = Content.Expression };
                var contentItemJson = JsonConvert.SerializeObject(await ScriptEvaluator.EvaluateAsync(expression, workflowContext));
                var res = JsonConvert.DeserializeObject<ContentItem>(contentItemJson);
                return res;
            }

            // If no expression was provided, see if the content item was provided as an input or as a property.
            var content = workflowContext.Input.GetValue<IContent>(ContentsHandler.ContentItemInputKey)
                ?? workflowContext.Properties.GetValue<IContent>(ContentsHandler.ContentItemInputKey);

            if (content != null)
            {
                return content;
            }

            return null;
        }
    }
}