using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Handlers;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class UpdateContentTask : ContentTask
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly JavaScriptEncoder _javaScriptEncoder;

        public UpdateContentTask(
            IContentManager contentManager, 
            IWorkflowExpressionEvaluator expressionEvaluator, 
            IWorkflowScriptEvaluator scriptEvaluator, 
            IStringLocalizer<UpdateContentTask> localizer,
            JavaScriptEncoder javaScriptEncoder)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _expressionEvaluator = expressionEvaluator;
            _javaScriptEncoder = javaScriptEncoder;
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
            return Outcomes(S["Done"], S["Failed"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItemId = await GetContentItemIdAsync(workflowContext);
            var contentItem = await ContentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (!String.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext, _javaScriptEncoder);
                contentItem.Merge(JObject.Parse(contentProperties), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
            }
            await ContentManager.UpdateAsync(contentItem);

            var result = await ContentManager.ValidateAsync(contentItem);
            if (result.Succeeded)
            {
                workflowContext.LastResult = contentItem;
                workflowContext.CorrelationId = contentItem.ContentItemId;
                workflowContext.Properties[ContentsHandler.ContentItemInputKey] = contentItem;

                return Outcomes("Done");
            }
            else
            {
                workflowContext.LastResult = result;

                return Outcomes("Failed");
            }
        }
    }
}
