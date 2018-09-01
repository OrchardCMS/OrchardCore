using System.Collections.Generic;
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
    public class CreateContentTask : ContentTask
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public CreateContentTask(IContentManager contentManager, IWorkflowExpressionEvaluator expressionEvaluator, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<CreateContentTask> localizer)
            : base(contentManager, scriptEvaluator, localizer)
        {
            _expressionEvaluator = expressionEvaluator;
        }

        public override string Name => nameof(CreateContentTask);
        public override LocalizedString Category => T["Content"];

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
            get => GetProperty(() => new WorkflowExpression<string>(JsonConvert.SerializeObject(new { TitlePart = new { Title = "" } }, Formatting.Indented)));
            set => SetProperty(value);
        }

        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return !string.IsNullOrEmpty(ContentType);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public async override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItem = await ContentManager.NewAsync(ContentType);

            if (!string.IsNullOrWhiteSpace(ContentProperties.Expression))
            {
                var contentProperties = await _expressionEvaluator.EvaluateAsync(ContentProperties, workflowContext);
                var propertyObject = JObject.Parse(contentProperties);

                ((JObject)contentItem.Content).Merge(propertyObject);
            }

            var versionOptions = Publish ? VersionOptions.Published : VersionOptions.Draft;
            await ContentManager.CreateAsync(contentItem, versionOptions);

            workflowContext.LastResult = contentItem;
            workflowContext.CorrelationId = contentItem.ContentItemId;
            workflowContext.Properties[ContentsHandler.ContentItemInputKey] = contentItem;

            return Outcomes("Done");
        }
    }
}