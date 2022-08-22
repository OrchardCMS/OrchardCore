using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Workflows;
using Newtonsoft.Json;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentForEachTask : TaskActivity
    {
        readonly IStringLocalizer S;
        private readonly ISession _session;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IContentManager _contentManager;

        public ContentForEachTask(IWorkflowScriptEvaluator scriptEvaluator, IContentManager contentManager, IStringLocalizer<RetrieveContentTask> localizer, ISession session)
        {
            S = localizer;
            _session = session;
            _scriptEvaluator = scriptEvaluator;
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
            //Store the result so a db call is not made per iteration
            if (Skip == 0 || Take % Index == 1)
            {
                ContentItems = await _session
                    .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentType && (Published && index.Published == true || !Published && index.Latest == true))
                    .Skip(Skip)
                    .Take(Take)
                    .ListAsync() as List<ContentItem>;
                Skip += Take;
                Index = 0;
            }

            if (ContentItems.Count() != 0)
            {
                var contentItem = ContentItems[Index];
                var current = Current = contentItem;
                workflowContext.CorrelationId = contentItem.ContentItemId;
                workflowContext.Properties[ContentEventConstants.ContentItemInputKey] = contentItem;
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
        /// The current skip number.
        /// </summary>
        public int Skip
        {
            get => GetProperty(() => 0);
            set => SetProperty(value);
        }
        
        /// <summary>
        /// How many to take each db call.
        /// </summary>
        public int Take
        {
            get => GetProperty(() => 10);
            set => SetProperty(value);
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

        /// <summary>
        /// The collection of contentItems.
        /// </summary>
        public List<ContentItem> ContentItems
        {
            get => GetProperty(() => new List<ContentItem>());
            set => SetProperty(value);
        }

        public string ContentType
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }

        public bool Published
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }
    }

}
