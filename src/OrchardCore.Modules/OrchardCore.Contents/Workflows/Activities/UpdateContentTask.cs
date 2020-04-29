using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class UpdateContentTask : ContentTask
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public UpdateContentTask(IContentManager contentManager, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<UpdateContentTask> localizer)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(UpdateContentTask);

        public override LocalizedString Category => S["Content"];

        public override LocalizedString DisplayText => S["Update Content Task"];

        public string ContentType
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ContentItemIdExpression
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> ContentProperties
        {
            get => GetProperty(() => new WorkflowExpression<string>(JsonConvert.SerializeObject(new { DisplayText = S["Enter a title"].Value }, Formatting.Indented)));
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return !String.IsNullOrEmpty(ContentType);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItemId = await GetContentItemIdAsync(workflowContext);

            // Use 'DraftRequired' so that we mutate a new version unless the type is not 'Versionable'.
            var contentItem = await ContentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                // If e.g. in the same scope of a related 'ContentCreatedEvent', the content item is not yet persisted
                // nor cached, so we fallback to the workflow context input that has been set by the 'ContentsHandler'.
                contentItem = workflowContext.Input.GetValue<IContent>(ContentsHandler.ContentItemInputKey)?.ContentItem;

                if (!String.Equals(contentItem?.ContentItemId, contentItemId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to retrieve the related content item.");
                }
            }

            if (!String.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext);
                contentItem.Merge(JObject.Parse(contentProperties), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
            }

            await ContentManager.UpdateAsync(contentItem);
            workflowContext.LastResult = contentItem;
            workflowContext.CorrelationId = contentItem.ContentItemId;
            workflowContext.Properties[ContentsHandler.ContentItemInputKey] = contentItem;
            return Outcomes("Done");
        }
    }
}
