using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Workflows.Indexes;
using YesSql;

namespace OrchardCore.Workflows.Routing
{
    public class WorkflowRouter : IRouter
    {
        private readonly IRouter _target;
        private static HashSet<string> _keys = new HashSet<string>(new[] { "area", "controller", "action", "workflowDefinitionId" }, StringComparer.OrdinalIgnoreCase);

        public WorkflowRouter(IRouter target)
        {
            _target = target;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (context.Values["area"]?.ToString() == "OrchardCore.Workflows" &&
                context.Values["controller"]?.ToString() == "Workflow" &&
                context.Values["action"]?.ToString() == "Invoke" &&
                context.Values["workflowDefinitionId"] != null)
            {
                var id = context.Values["workflowDefinitionId"] as int?;

                if (id != null)
                {
                    // TODO: Implement a caching strategy like Autoroute did.
                    var session = context.HttpContext.RequestServices.GetRequiredService<ISession>();
                    var index = session.QueryIndex<WorkflowDefinitionByHttpRequestIndex>(x => x.WorkflowDefinitionId == id).FirstOrDefaultAsync().GetAwaiter().GetResult();

                    if (index != null)
                    {
                        var path = index.RequestPath;

                        // Append any querystring parameters.
                        if (context.Values.Count > 4)
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
            }

            return null;
        }

        public async Task RouteAsync(RouteContext context)
        {
            var requestPath = context.HttpContext.Request.Path.Value;
            var session = context.HttpContext.RequestServices.GetRequiredService<ISession>();
            var index = await session.QueryIndex<WorkflowDefinitionByHttpRequestIndex>(x => x.RequestPath == requestPath).FirstOrDefaultAsync();

            if (index != null)
            {
                context.RouteData.Values["area"] = "OrchardCore.Workflows";
                context.RouteData.Values["controller"] = "Workflow";
                context.RouteData.Values["action"] = "Invoke";
                context.RouteData.Values["workflowDefinitionId"] = index.WorkflowDefinitionId;
                context.RouteData.Values["activityId"] = index.ActivityId;

                context.RouteData.Routers.Add(_target);
                await _target.RouteAsync(context);
            }
        }
    }
}
