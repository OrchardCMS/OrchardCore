using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentForEachTask : TaskActivity
    {
        readonly IStringLocalizer S;
        private readonly ISession _session;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IContentManager _contentManager;
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;

        public ContentForEachTask(IWorkflowExpressionEvaluator expressionEvaluator, IContentManager contentManager, IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<RetrieveContentTask> localizer, ISession session)
        {
            S = localizer;
            _session = session;
            _scriptEvaluator = scriptEvaluator;
            _expressionEvaluator = expressionEvaluator;
            _contentManager = contentManager;
        }
        public override string Name => nameof(ContentForEachTask);

        public override LocalizedString DisplayText => S["Content For Each Task"];

        public override LocalizedString Category => S["Content"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(S["Iterate"], S["Done"]);
        }
        public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return true;
        }
        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var contentItems = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentType && index.Published == true && index.Latest == true)
                .ListAsync();
            List<ContentItem> items = contentItems.ToList();
            if (contentItems.Count() == 0)
            {
                throw new InvalidOperationException($"The '{nameof(ContentForEachTask)}' failed to evaluate the 'ContentType'.");
            }

            if (Index < contentItems.Count())
            {
                var contentItem = await _contentManager.LoadAsync(items[Index]);
                var current = Current = contentItem;
                workflowContext.CorrelationId = contentItem.ContentItemId;
                workflowContext.Properties["ContentItem"] = contentItem;
                workflowContext.LastResult = current;
                Index++;
                return Outcomes("Iterate"); 
            }
            else
            {
                Index = 0;
                return Outcomes("Done");
            }
        }

        /// <summary>
        /// The current number of iterations executed.
        /// </summary>
        public int Index
        {
            get => GetProperty(() => 0);
            set => SetProperty(value);
        }

        /// <summary>
        /// The current iteration value.
        /// </summary>
        public object Current
        {
            get => GetProperty<object>();
            set => SetProperty(value);
        }

        public string ContentType
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
    }

}
