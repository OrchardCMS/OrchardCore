using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Extensions;

public static class HttpContextExtensions
{
    public static async Task<ActionContext> GetActionContextAsync(this IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var actionContext = httpContext.RequestServices.GetService<IActionContextAccessor>()?.ActionContext;

        if (actionContext != null)
        {
            return actionContext;
        }

        return await GetActionContextAsync(httpContext);
    }

    public static async Task<ActionContext> GetActionContextAsync(this HttpContext httpContext)
    {
        var routeData = new RouteData();
        routeData.Routers.Add(new RouteCollection());

        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        var filters = httpContext.RequestServices.GetServices<IAsyncViewActionFilter>();

        foreach (var filter in filters)
        {
            await filter.OnActionExecutionAsync(actionContext);
        }

        return actionContext;
    }
}
