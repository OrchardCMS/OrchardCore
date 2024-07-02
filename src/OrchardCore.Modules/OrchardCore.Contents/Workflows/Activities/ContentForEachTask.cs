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
using OrchardCore.Queries;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Descriptor.Models;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentForEachTask : TaskActivity
    {
        readonly IStringLocalizer S;
        private readonly ISession _session;
        private readonly IWorkflowScriptEvaluator _scriptEvaluator;
        private readonly IContentManager _contentManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IServiceProvider _serviceProvider;

        public ContentForEachTask(IWorkflowScriptEvaluator scriptEvaluator, IContentManager contentManager, IStringLocalizer<ContentForEachTask> localizer, ISession session, ShellDescriptor shellDescriptor, IServiceProvider serviceProvider)
        {
            _scriptEvaluator = scriptEvaluator;
            _contentManager = contentManager;
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
            //Only try and get service if feature is enabled.
            if (UseQuery && _shellDescriptor.Features.Any(feature => feature.Id == "OrchardCore.Queries"))
            {
                var _queryManager = (IQueryManager)_serviceProvider.GetService(typeof(IQueryManager));
                if (Index % Take == 0)
                {
                    //Query is selected, get the query, 
                    var contentItems = new List<ContentItem>();                  
                    dynamic query = await _queryManager.GetQueryAsync(Query);
                    if (query == null)
                    {
                        throw new InvalidOperationException($"Failed to retrieve the query {Query} (Have you changed or deleted the query?)");
                    }
                    var queryParameters = !String.IsNullOrEmpty(Parameters) ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(Parameters)
                        : new Dictionary<string, object>();
                    //Override skip and take in the query to enable paging of datasource
                    var template = JObject.Parse(query.Template);
                    template["from"] = Skip;
                    template["size"] = Take;
                    query.Template = template.ToString();
                    //Store the contentitems or the query results in the workflow context (Return document may not be selected)
                    var results = (await _queryManager.ExecuteQueryAsync(query, queryParameters)).Items;
                    if (results != null)
                    {
                        foreach (var result in results)
                        {
                            if (!(result is ContentItem contentItem))
                            {
                                contentItem = null;

                                if (result is JObject jObject)
                                {
                                    contentItem = jObject.ToObject<ContentItem>();
                                }
                            }
                            if (contentItem?.ContentItemId == null)
                            {
                                continue;
                            }
                            contentItems.Add(contentItem);
                        }
                    }
                    Skip += Take;
                    Index = 0;
                    ContentItems = contentItems;
                }
            }
            //Query the db for a contenttype and store the result so a db call is not made per iteration
            else if (Index % Take == 0)
            {
                ContentItems = await _session
                .Query<ContentItem, ContentItemIndex>(index => index.ContentType == ContentType)
                //If PublishedOnly == true then Publish should be true and Latest can be either and vice versa
                .Where(w => (w.Published == true || w.Published == PublishedOnly) && (w.Latest == true || w.Latest == !PublishedOnly))
                .Skip(Skip)
                .Take(Take)
                .ListAsync() as List<ContentItem>;
                Skip += Take;
                Index = 0;
            }

            if (ContentItems.Count() != 0 && Index < ContentItems.Count())
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
        public string Parameters
        {
            get => GetProperty(() => string.Empty);
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
