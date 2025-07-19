using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Extensions;

public static class HttpContextExtensions
{
    public static Task<ActionContext> GetActionContextAsync(this IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        
        return GetActionContextAsync(httpContext);
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
