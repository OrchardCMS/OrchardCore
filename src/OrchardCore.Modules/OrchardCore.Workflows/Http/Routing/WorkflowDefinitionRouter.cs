using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Services;

namespace OrchardCore.Workflows.Http.Routing
{
    public class WorkflowDefinitionRouter : IRouter
    {
        private readonly IRouter _target;
        private static HashSet<string> _keys = new HashSet<string>(new[] { "area", "controller", "action", "workflowDefinitionId", "activityId", "correlationId" }, StringComparer.OrdinalIgnoreCase);

        public WorkflowDefinitionRouter(IRouter target)
        {
            _target = target;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (context.Values["area"]?.ToString() == "OrchardCore.Workflows" &&
                context.Values["controller"]?.ToString() == "Workflow" &&
                context.Values["action"]?.ToString() == "Start" &&
                context.Values["workflowDefinitionId"] != null &&
                context.Values["activityId"] != null)
            {
                var workflowDefinitionId = context.Values["workflowDefinitionId"].ToString();
                var activityId = (int)context.Values["activityId"];
                var correlationId = context.Values.GetValue<string>("correlationId");
                var httpMethod = context.HttpContext.Request.Method;
                var workflowDefinitionPathEntries = context.HttpContext.RequestServices.GetRequiredService<IWorkflowDefinitionPathEntries>();
                var entry = workflowDefinitionPathEntries.GetEntry(httpMethod, workflowDefinitionId, activityId, correlationId);

                if (entry != null)
                {
                    var path = entry.Path;

                    // Append any querystring parameters.
                    if (context.Values.Count > _keys.Count)
                    {
                        foreach (var data in context.Values)
                        {
                            if (!_keys.Contains(data.Key))
                            {
                                path = QueryHelpers.AddQueryString(path, data.Key, data.Value.ToString());
                            }
                        }
                    }

                    return new VirtualPathData(_target, path);
                }
            }

            return null;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var requestPath = context.HttpContext.Request.Path.Value;
            var httpMethod = context.HttpContext.Request.Method;
            var workflowDefinitionPathEntries = context.HttpContext.RequestServices.GetRequiredService<IWorkflowDefinitionPathEntries>();
            var entries = workflowDefinitionPathEntries.GetEntries(httpMethod, requestPath).ToList();

            if (entries.Count > 1)
            {
                throw new InvalidProgramException($"There are multiple workflows that handle path '{requestPath}'. Multiple workflows handling the same path is not supported.");
            }

            if (entries.Count == 1)
            {
                var entry = entries.First();
                context.RouteData.Values["area"] = "OrchardCore.Workflows";
                context.RouteData.Values["controller"] = "Workflow";
                context.RouteData.Values["action"] = "Start";
                context.RouteData.Values["workflowDefinitionId"] = entry.WorkflowId;
                context.RouteData.Values["activityId"] = entry.ActivityId;

                context.RouteData.Routers.Add(_target);
                await _target.RouteAsync(context);
            }
        }
    }
}
