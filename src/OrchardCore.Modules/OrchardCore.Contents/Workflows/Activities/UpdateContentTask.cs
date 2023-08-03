using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
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
        private readonly JavaScriptEncoder _javaScriptEncoder;

        public UpdateContentTask(
            IContentManager contentManager,
            IUpdateModelAccessor updateModelAccessor,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            IStringLocalizer<UpdateContentTask> localizer,
            JavaScriptEncoder javaScriptEncoder)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _updateModelAccessor = updateModelAccessor;
            _expressionEvaluator = expressionEvaluator;
            _javaScriptEncoder = javaScriptEncoder;
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
            var contentItemId = (await GetContentItemIdAsync(workflowContext))
                ?? throw new InvalidOperationException($"The {nameof(UpdateContentTask)} failed to evaluate the 'ContentItemId'.");

            var inlineEventOfSameContentItemId = String.Equals(InlineEvent.ContentItemId, contentItemId, StringComparison.OrdinalIgnoreCase);

            if (inlineEventOfSameContentItemId)
            {
                if (InlineEvent.Name == nameof(ContentPublishedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(UpdateContentTask)}' can't update the content item as it is executed inline from a '{nameof(ContentPublishedEvent)}' of the same content item, please use an event that is triggered earlier.");
                }

                if (InlineEvent.Name == nameof(ContentDraftSavedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(UpdateContentTask)}' can't update the content item as it is executed inline from a '{nameof(ContentDraftSavedEvent)}' of the same content item, please use an event that is triggered earlier.");
                }
            }

            ContentItem contentItem = null;

            if (!inlineEventOfSameContentItemId)
            {
                contentItem = await ContentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);
            }
            else
            {
                contentItem = workflowContext.Input.GetValue<IContent>(ContentEventConstants.ContentItemInputKey)?.ContentItem;
            }

            if (contentItem == null)
            {
                throw new InvalidOperationException($"The '{nameof(UpdateContentTask)}' failed to retrieve the content item.");
            }

            if (!inlineEventOfSameContentItemId && InlineEvent.IsStart && InlineEvent.ContentType == contentItem.ContentType)
            {
                if (InlineEvent.Name == nameof(ContentUpdatedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(UpdateContentTask)}' can't update the content item as it is executed inline from a starting '{nameof(ContentUpdatedEvent)}' of the same content type, which would result in an infinitive loop.");
                }

                if (Publish && InlineEvent.Name == nameof(ContentPublishedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(UpdateContentTask)}' can't publish the content item as it is executed inline from a starting '{nameof(ContentPublishedEvent)}' of the same content type, which would result in an infinitive loop.");
                }

                if (!Publish && InlineEvent.Name == nameof(ContentDraftSavedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(UpdateContentTask)}' can't update the content item as it is executed inline from a starting '{nameof(ContentDraftSavedEvent)}' of the same content type, which would result in an infinitive loop.");
                }
            }

            if (!String.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext, _javaScriptEncoder);
                contentItem.Merge(JObject.Parse(contentProperties), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
            }

            if (!inlineEventOfSameContentItemId)
            {
                await ContentManager.UpdateAsync(contentItem);
            }

            var result = await ContentManager.ValidateAsync(contentItem);

            if (result.Succeeded)
            {
                if (!inlineEventOfSameContentItemId)
                {
                    if (Publish)
                    {
                        await ContentManager.PublishAsync(contentItem);
                    }
                    else
                    {
                        await ContentManager.SaveDraftAsync(contentItem);
                    }
                }

                workflowContext.CorrelationId = contentItem.ContentItemId;
                workflowContext.Properties[ContentEventConstants.ContentItemInputKey] = contentItem;
                workflowContext.LastResult = contentItem;

                return Outcomes("Done");
            }

            if (inlineEventOfSameContentItemId)
            {
                _updateModelAccessor.ModelUpdater.ModelState.AddModelError(nameof(UpdateContentTask),
                    $"The '{workflowContext.WorkflowType.Name}:{nameof(UpdateContentTask)}' failed to update the content item: "
                    + String.Join(", ", result.Errors));
            }

            workflowContext.LastResult = result;

            return Outcomes("Failed");
        }
    }
}
