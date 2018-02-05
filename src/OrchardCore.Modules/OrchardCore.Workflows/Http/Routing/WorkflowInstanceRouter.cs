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
    public class WorkflowInstanceRouter : IRouter
    {
        private readonly IRouter _target;
        private static HashSet<string> _keys = new HashSet<string>(new[] { "area", "controller", "action", "workflowInstanceId", "activityId" }, StringComparer.OrdinalIgnoreCase);

        public WorkflowInstanceRouter(IRouter target)
        {
            _target = target;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (context.Values["area"]?.ToString() == "OrchardCore.Workflows" &&
                context.Values["controller"]?.ToString() == "Workflow" &&
                context.Values["action"]?.ToString() == "Resume" &&
                context.Values["uid"] != null &&
                context.Values["activityId"] != null)
            {
                var workflowInstanceUid = (string)context.Values["workflowInstanceUid"];
                var activityId = (int)context.Values["activityId"];
                var httpMethod = context.HttpContext.Request.Method;
                var workflowInstancePathEntries = context.HttpContext.RequestServices.GetRequiredService<IWorkflowInstancePathEntries>();
                var entry = workflowInstancePathEntries.GetEntry(httpMethod, workflowInstanceUid, activityId);

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
            var workflowInstancePathEntries = context.HttpContext.RequestServices.GetRequiredService<IWorkflowInstancePathEntries>();
            var workflowInstanceUid = context.HttpContext.Request.Query["uid"];
            var correlationId = context.HttpContext.Request.Query["correlationId"];
            var query = workflowInstancePathEntries.GetEntries(httpMethod, requestPath);

            if (!string.IsNullOrWhiteSpace(workflowInstanceUid))
            {
                query = query.Where(x => x.WorkflowId == workflowInstanceUid);
            }
            else if (!string.IsNullOrWhiteSpace(correlationId))
            {
                query = query.Where(x => string.Equals(x.CorrelationId, correlationId, StringComparison.OrdinalIgnoreCase));
            }

            var entry = query.FirstOrDefault();

            if (entry != null)
            {
                context.RouteData.Values["area"] = "OrchardCore.Workflows";
                context.RouteData.Values["controller"] = "Workflow";
                context.RouteData.Values["action"] = "Resume";
                context.RouteData.Values["uid"] = entry.WorkflowId;
                context.RouteData.Values["activityId"] = entry.ActivityId;

                context.RouteData.Routers.Add(_target);
                await _target.RouteAsync(context);
            }
        }
    }
}
