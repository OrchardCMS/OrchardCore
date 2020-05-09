using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Http.Services
{
    internal class WorkflowRouteEntries : IWorkflowInstanceRouteEntries
    {
        public WorkflowRouteEntries()
        {
        }

        public async Task<IEnumerable<WorkflowRoutesEntry>> GetWorkflowRouteEntriesAsync(string httpMethod, RouteValueDictionary routeValues)
        {
            var document = await GetDocumentAsync();
            return GetWorkflowRoutesEntries(document, httpMethod, routeValues);
        }

        public async Task AddEntriesAsync(IEnumerable<WorkflowRoutesEntry> entries)
        {
            var document = await LoadDocumentAsync();
            AddEntries(document, entries);
            await DocumentManager.UpdateAsync(document);
        }

        public async Task RemoveEntriesAsync(string workflowId)
        {
            var document = await LoadDocumentAsync();
            RemoveEntries(document, workflowId);
            await DocumentManager.UpdateAsync(document);
        }

        public void AddEntries(WorkflowRouteDocument document, IEnumerable<WorkflowRoutesEntry> entries)
        {
            foreach (var group in entries.GroupBy(x => x.WorkflowId))
            {
                document.Entries[group.Key] = group.ToList();
            }
        }

        public void RemoveEntries(WorkflowRouteDocument document, string workflowId) => document.Entries.Remove(workflowId);

        /// <summary>
        /// Loads the workflow route document for updating and that should not be cached.
        /// </summary>
        private Task<WorkflowRouteDocument> LoadDocumentAsync() => DocumentManager.GetMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the workflow route document for sharing and that should not be updated.
        /// </summary>
        private Task<WorkflowRouteDocument> GetDocumentAsync() => DocumentManager.GetImmutableAsync(CreateDocumentAsync);

        private async Task<WorkflowRouteDocument> CreateDocumentAsync()
        {
            var workflowTypeDictionary = (await Session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync()).ToDictionary(x => x.WorkflowTypeId);

            var skip = 0;
            var pageSize = 50;
            var document = new WorkflowRouteDocument();

            while (true)
            {
                var pendingWorkflows = await Session
                    .Query<Workflow, WorkflowBlockingActivitiesIndex>(index =>
                        index.ActivityName == HttpRequestFilterEvent.EventName)
                    .Skip(skip)
                    .Take(pageSize)
                    .ListAsync();

                if (!pendingWorkflows.Any())
                {
                    break;
                }

                var workflowRouteEntries =
                    from workflow in pendingWorkflows
                    from entry in GetWorkflowRoutesEntries(workflowTypeDictionary[workflow.WorkflowTypeId], workflow, ActivityLibrary)
                    select entry;

                AddEntries(document, workflowRouteEntries);

                if (pendingWorkflows.Count() < pageSize)
                {
                    break;
                }

                skip += pageSize;
            }

            return document;
        }

        internal static IEnumerable<WorkflowRoutesEntry> GetWorkflowRoutesEntries(WorkflowType workflowType, Workflow workflow, IActivityLibrary activityLibrary)
        {
            var awaitingActivityIds = workflow.BlockingActivities.Select(x => x.ActivityId).ToDictionary(x => x);
            return workflowType.Activities.Where(x => x.Name == HttpRequestFilterEvent.EventName && awaitingActivityIds.ContainsKey(x.ActivityId)).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflow.WorkflowId,
                    ActivityId = x.ActivityId,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues,
                };

                return entry;
            });
        }

        internal static IEnumerable<WorkflowRoutesEntry> GetWorkflowRoutesEntries(WorkflowRouteDocument document, string httpMethod, RouteValueDictionary routeValues)
        {
            var controllerName = routeValues.GetValue<string>("controller");
            var actionName = routeValues.GetValue<string>("action");
            var areaName = routeValues.GetValue<string>("area");

            return document.Entries.Values.SelectMany(x => x).Where(x =>
                x.HttpMethod == httpMethod
                && (x.ControllerName == controllerName || string.IsNullOrWhiteSpace(x.ControllerName))
                && (x.ActionName == actionName || string.IsNullOrWhiteSpace(x.ActionName))
                && (x.AreaName == areaName || string.IsNullOrWhiteSpace(x.AreaName)))
                .ToArray();
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IActivityLibrary ActivityLibrary => ShellScope.Services.GetRequiredService<IActivityLibrary>();

        private static IVolatileDocumentManager<WorkflowRouteDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<WorkflowRouteDocument>>();
    }
}
