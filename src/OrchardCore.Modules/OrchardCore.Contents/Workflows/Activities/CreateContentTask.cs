using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class CreateContentTask : ContentTask
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public CreateContentTask(IContentManager contentManager, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<CreateContentTask> localizer)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(CreateContentTask);

        public override LocalizedString Category => S["Content"];

        public override LocalizedString DisplayText => S["Create Content Task"];

        public string ContentType
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public bool Publish
        {
            get => GetProperty<bool>();
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
            return Outcomes(S["Done"], S["Failed"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            if (IsInlineFromStartingContentEventOfSameContentType(ContentType))
            {
                if (ContentEventInfo.Name == nameof(ContentUpdatedEvent))
                {
                    throw new InvalidOperationException($"The '{workflowContext.WorkflowType.Name}:{DisplayText}' can't update the content item as it is executed inline from a starting '{nameof(ContentUpdatedEvent.DisplayText)}' of the same content type.");
                }

                if (ContentEventInfo.Name == nameof(ContentCreatedEvent))
                {
                    throw new InvalidOperationException($"The '{workflowContext.WorkflowType.Name}:{DisplayText}' can't create the content item as it is executed inline from a starting '{nameof(ContentCreatedEvent.DisplayText)}' of the same content type.");
                }

                if (Publish && ContentEventInfo.Name == nameof(ContentPublishedEvent))
                {
                    throw new InvalidOperationException($"The '{workflowContext.WorkflowType.Name}:{DisplayText}' can't publish the content item as it is executed inline from a starting '{nameof(ContentPublishedEvent.DisplayText)}' of the same content type.");
                }
            }

            var contentItem = await ContentManager.NewAsync(ContentType);

            if (!String.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext);
                contentItem.Merge(JObject.Parse(contentProperties));
            }

            var result = await ContentManager.UpdateValidateAndCreateAsync(contentItem, Publish ? VersionOptions.Published : VersionOptions.Draft);

            if (result.Succeeded)
            {
                workflowContext.CorrelationId = contentItem.ContentItemId;
                workflowContext.Properties[ContentEventConstants.ContentItemInputKey] = contentItem;
                workflowContext.LastResult = contentItem;

                return Outcomes("Done");
            }

            workflowContext.LastResult = result;

            return Outcomes("Failed");
        }
    }
}
