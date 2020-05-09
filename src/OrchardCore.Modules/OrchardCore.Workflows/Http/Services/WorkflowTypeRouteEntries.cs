using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Http.Services
{
    internal class WorkflowTypeRouteEntries : IWorkflowTypeRouteEntries
    {
        public WorkflowTypeRouteEntries()
        {
        }

        public async Task<IEnumerable<WorkflowRoutesEntry>> GetWorkflowRouteEntriesAsync(string httpMethod, RouteValueDictionary routeValues)
        {
            var document = await GetDocumentAsync();
            return WorkflowRouteEntries.GetWorkflowRoutesEntries(document, httpMethod, routeValues);
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

        public void AddEntries(WorkflowTypeRouteDocument document, IEnumerable<WorkflowRoutesEntry> entries)
        {
            foreach (var group in entries.GroupBy(x => x.WorkflowId))
            {
                document.Entries[group.Key] = group.ToList();
            }
        }

        public void RemoveEntries(WorkflowTypeRouteDocument document, string workflowId) => document.Entries.Remove(workflowId);

        /// <summary>
        /// Loads the workflow type route document for updating and that should not be cached.
        /// </summary>
        private Task<WorkflowTypeRouteDocument> LoadDocumentAsync() => DocumentManager.GetMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the workflow type route document for sharing and that should not be updated.
        /// </summary>
        private Task<WorkflowTypeRouteDocument> GetDocumentAsync() => DocumentManager.GetImmutableAsync(CreateDocumentAsync);

        private async Task<WorkflowTypeRouteDocument> CreateDocumentAsync()
        {
            var workflowTypeDictionary = (await Session.Query<WorkflowType, WorkflowTypeIndex>().ListAsync()).ToDictionary(x => x.WorkflowTypeId);

            var workflowTypeRouteEntries =
                from workflowType in workflowTypeDictionary.Values
                from entry in GetWorkflowTypeRoutesEntries(workflowType, ActivityLibrary)
                select entry;

            var document = new WorkflowTypeRouteDocument();
            AddEntries(document, workflowTypeRouteEntries);

            return document;
        }

        internal static IEnumerable<WorkflowRoutesEntry> GetWorkflowTypeRoutesEntries(WorkflowType workflowType, IActivityLibrary activityLibrary)
        {
            return workflowType.Activities.Where(x => x.IsStart && x.Name == HttpRequestFilterEvent.EventName).Select(x =>
            {
                var activity = activityLibrary.InstantiateActivity<HttpRequestFilterEvent>(x);
                var entry = new WorkflowRoutesEntry
                {
                    WorkflowId = workflowType.Id.ToString(),
                    ActivityId = x.ActivityId,
                    HttpMethod = activity.HttpMethod,
                    RouteValues = activity.RouteValues
                };

                return entry;
            });
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IActivityLibrary ActivityLibrary => ShellScope.Services.GetRequiredService<IActivityLibrary>();

        private static IVolatileDocumentManager<WorkflowTypeRouteDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<WorkflowTypeRouteDocument>>();
    }
}
