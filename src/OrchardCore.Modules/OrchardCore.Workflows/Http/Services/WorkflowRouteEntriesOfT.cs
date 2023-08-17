using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Services;
using YesSql;

namespace OrchardCore.Workflows.Http.Services
{
    internal class WorkflowRouteEntries<TWorkflowRouteDocument> : IWorkflowRouteEntries where TWorkflowRouteDocument : WorkflowRouteDocument, new()
    {
        private readonly IVolatileDocumentManager<TWorkflowRouteDocument> _documentManager;

        public WorkflowRouteEntries(IVolatileDocumentManager<TWorkflowRouteDocument> documentManager)
        {
            _documentManager = documentManager;
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
            await _documentManager.UpdateAsync(document);
        }

        public async Task RemoveEntriesAsync(string workflowId)
        {
            var document = await LoadDocumentAsync();
            RemoveEntries(document, workflowId);
            await _documentManager.UpdateAsync(document);
        }

        protected virtual IEnumerable<WorkflowRoutesEntry> GetWorkflowRoutesEntries(WorkflowRouteDocument document, string httpMethod, RouteValueDictionary routeValues)
        {
            var controllerName = routeValues.GetValue<string>("controller");
            var actionName = routeValues.GetValue<string>("action");
            var areaName = routeValues.GetValue<string>("area");

            return document.Entries.Values.SelectMany(x => x).Where(x =>
                x.HttpMethod == httpMethod &&
                (x.ControllerName == controllerName || String.IsNullOrWhiteSpace(x.ControllerName)) &&
                (x.ActionName == actionName || String.IsNullOrWhiteSpace(x.ActionName)) &&
                (x.AreaName == areaName || String.IsNullOrWhiteSpace(x.AreaName)))
                .ToArray();
        }

        public static void AddEntries(TWorkflowRouteDocument document, IEnumerable<WorkflowRoutesEntry> entries)
        {
            foreach (var group in entries.GroupBy(x => x.WorkflowId))
            {
                document.Entries[group.Key] = group.ToList();
            }
        }

        public static void RemoveEntries(TWorkflowRouteDocument document, string workflowId) => document.Entries.Remove(workflowId);

        /// <summary>
        /// Loads the workflow route document for updating and that should not be cached.
        /// </summary>
        private Task<TWorkflowRouteDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the workflow route document for sharing and that should not be updated.
        /// </summary>
        private Task<TWorkflowRouteDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync(CreateDocumentAsync);

        protected virtual Task<TWorkflowRouteDocument> CreateDocumentAsync() => Task.FromResult(new TWorkflowRouteDocument());

        protected static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        protected static IActivityLibrary ActivityLibrary => ShellScope.Services.GetRequiredService<IActivityLibrary>();
    }
}
