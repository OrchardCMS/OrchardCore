using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
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
        private readonly JavaScriptEncoder _javaScriptEncoder;

        public CreateContentTask(
            IContentManager contentManager,
            IWorkflowExpressionEvaluator expressionEvaluator,
            IWorkflowScriptEvaluator scriptEvaluator,
            IStringLocalizer<CreateContentTask> localizer,
            JavaScriptEncoder javaScriptEncoder)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _expressionEvaluator = expressionEvaluator;
            _javaScriptEncoder = javaScriptEncoder;
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
            if (InlineEvent.IsStart && InlineEvent.ContentType == ContentType)
            {
                if (InlineEvent.Name == nameof(ContentUpdatedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(CreateContentTask)}' can't update the content item as it is executed inline from a starting '{nameof(ContentUpdatedEvent)}' of the same content type, which would result in an infinitive loop.");
                }

                if (InlineEvent.Name == nameof(ContentCreatedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(CreateContentTask)}' can't create the content item as it is executed inline from a starting '{nameof(ContentCreatedEvent)}' of the same content type, which would result in an infinitive loop.");
                }

                if (Publish && InlineEvent.Name == nameof(ContentPublishedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(CreateContentTask)}' can't publish the content item as it is executed inline from a starting '{nameof(ContentPublishedEvent)}' of the same content type, which would result in an infinitive loop.");
                }

                if (!Publish && InlineEvent.Name == nameof(ContentDraftSavedEvent))
                {
                    throw new InvalidOperationException($"The '{nameof(CreateContentTask)}' can't create the content item as it is executed inline from a starting '{nameof(ContentDraftSavedEvent)}' of the same content type, which would result in an infinitive loop.");
                }
            }

            var contentItem = await ContentManager.NewAsync(ContentType);

            if (!String.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext, _javaScriptEncoder);
                contentItem.Merge(JObject.Parse(contentProperties));
            }

            var result = await ContentManager.UpdateValidateAndCreateAsync(contentItem, VersionOptions.Draft);

            if (result.Succeeded)
            {
                if (Publish)
                {
                    await ContentManager.PublishAsync(contentItem);
                }
                else
                {
                    await ContentManager.SaveDraftAsync(contentItem);
                }

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
