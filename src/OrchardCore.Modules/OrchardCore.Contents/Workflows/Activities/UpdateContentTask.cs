using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class UpdateContentTask : ContentTask
    {
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public UpdateContentTask(
            IContentManager contentManager,
            IUpdateModelAccessor updateModelAccessor,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            IStringLocalizer<UpdateContentTask> localizer)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _updateModelAccessor = updateModelAccessor;
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(UpdateContentTask);

        public override LocalizedString Category => S["Content"];

        public override LocalizedString DisplayText => S["Update Content Task"];

        public bool Publish
        {
            get => GetProperty<bool>();
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

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Done"], S["Failed"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItemId = await GetContentItemIdAsync(workflowContext);

            if (contentItemId == null)
            {
                throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to evaluate the 'ContentItemId'.");
            }

            ContentItem contentItem = null;

            if (!InlineFromContentEvent)
            {
                contentItem = await ContentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
            }
            else
            {
                contentItem = workflowContext.Input.GetValue<IContent>(ContentItemInputKey)?.ContentItem;
            }

            if (contentItem == null)
            {
                throw new InvalidOperationException($"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to retrieve the content item.");
            }

            if (!String.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext);
                contentItem.Merge(JObject.Parse(contentProperties), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
            }

            if (!InlineFromContentEvent)
            {
                await ContentManager.UpdateAsync(contentItem);
            }

            var result = await ContentManager.ValidateAsync(contentItem);

            if (result.Succeeded)
            {
                if (Publish && !InlineFromContentEvent)
                {
                    await ContentManager.PublishAsync(contentItem);
                }

                workflowContext.CorrelationId = contentItem.ContentItemId;
                workflowContext.Properties[ContentItemInputKey] = contentItem;
                workflowContext.LastResult = contentItem;

                return Outcomes("Done");
            }

            if (InlineFromContentEvent)
            {
                _updateModelAccessor.ModelUpdater.ModelState.AddModelError(nameof(UpdateContentTask),
                    $"The {workflowContext.WorkflowType.Name}:{DisplayText} activity failed to update the content item: "
                    + String.Join(", ", result.Errors));
            }

            workflowContext.LastResult = result;

            return Outcomes("Failed");
        }
    }
}
