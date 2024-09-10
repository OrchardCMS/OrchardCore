using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Queries;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentForEachTask : TaskActivity
    {
        readonly IStringLocalizer S;
        private readonly ISession _session;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IServiceProvider _serviceProvider;
        private int _currentPage;

        public ContentForEachTask(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<ContentForEachTask> localizer, ISession session, ShellDescriptor shellDescriptor, IServiceProvider serviceProvider)
        {
            _scriptEvaluator = scriptEvaluator;
            _session = session;
            S = localizer;
            _shellDescriptor = shellDescriptor;
            _serviceProvider = serviceProvider;
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

            if (UseQuery && !_shellDescriptor.Features.Any(feature => feature.Id == "OrchardCore.Queries"))
            {
                throw new InvalidOperationException($"The '{nameof(ContentForEachTask)}' can't process the query as the feature OrchardCore.Queries is not enabled");
            }

            if (Index >= ContentItems.Count && !await FetchNextBatchAsync(workflowContext))
            {
                return Outcomes("Done");
            }

            ProcessContentItem(workflowContext);
            return Outcomes("Iterate");
        }

        private async Task<bool> FetchNextBatchAsync(WorkflowExecutionContext workflowContext)
        {
            //Have already looped once and there is no PageSize parameter so all results would have come with first query.
            if (_currentPage > 0 && PageSize == 0)
            {
                return false;
            }
            if (UseQuery)
            {
                ContentItems = await ExecContentQueryAsync(workflowContext);
            }
            else
            {
                await ExecuteContentTypeQueryAsync();
            }
            _currentPage++;
            Index = 0;
            return ContentItems.Count > 0;
        }

        private async Task<List<ContentItem>> ExecContentQueryAsync(WorkflowExecutionContext workflowContext)
        {
            var _queryManager = (IQueryManager)_serviceProvider.GetService(typeof(IQueryManager));
            var contentItems = new List<ContentItem>();
            Query query = await _queryManager.GetQueryAsync(Query);
            if (query == null)
            {
                throw new InvalidOperationException(S[$"Failed to retrieve the query {Query} (Have you changed, deleted the query or disabled the feature?)"]);
            }

            string queryParameters = await _scriptEvaluator.EvaluateAsync(Parameters, workflowContext, null);

            var parameters = !string.IsNullOrEmpty(queryParameters) ?
                JsonSerializer.Deserialize<Dictionary<string, object>>(queryParameters)
                : new Dictionary<string, object>();

            if (PageSize > 0)
            {
                parameters["from"] = _currentPage * PageSize;
                parameters["size"] = PageSize;
            }
            try
            {
                IQueryResults results = await _queryManager.ExecuteQueryAsync(query, parameters);
                foreach (ContentItem item in results.Items)
                {
                    contentItems.Add(item);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(S[$"Failed to run the query {Query}, failed with message: {e.Message}."]);
            }
            return contentItems;
        }

        private async Task ExecuteContentTypeQueryAsync()
        {
            ContentItems = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentType)
                .Where(w => (w.Published || w.Published == PublishedOnly) && (w.Latest || w.Latest == !PublishedOnly))
                .Skip(_currentPage * PageSize)
                .Take(PageSize)
                .ListAsync() as List<ContentItem>;
        }
        private void ProcessContentItem(WorkflowExecutionContext workflowContext)
        {
            var contentItem = ContentItems[Index];
            Current = contentItem;
            workflowContext.CorrelationId = contentItem.ContentItemId;
            workflowContext.Properties[ContentEventConstants.ContentItemInputKey] = contentItem;
            workflowContext.LastResult = Current;
            Index++;
        }

        /// <summary>
        /// How many to take each db call.
        /// </summary>
        public int PageSize
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
        /// <summary>
        /// The name of the content type to select.
        /// </summary>
        public string ContentType
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// Toggles between using a query (I.e. Lucene or raw YesSql query).
        /// </summary>
        public bool UseQuery
        {
            get => GetProperty<bool>(() => false);
            set => SetProperty(value);
        }
        /// <summary>
        /// The selected query source, if any.
        /// </summary>
        public string QuerySource
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// The name of the query to run.
        /// </summary>
        public string Query
        {
            get => GetProperty(() => string.Empty);
            set => SetProperty(value);
        }
        /// <summary>
        /// Parameters to pass into the query.
        /// </summary>
        public WorkflowExpression<string> Parameters
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }
        /// <summary>
        /// Only return published items.
        /// </summary>
        public bool PublishedOnly
        {
            get => GetProperty<bool>(() => false);
            set => SetProperty(value);
        }
    }

}
