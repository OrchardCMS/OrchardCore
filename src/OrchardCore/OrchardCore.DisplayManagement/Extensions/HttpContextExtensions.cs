using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Extensions;

public static class HttpContextExtensions
{
    public static ValueTask<ActionContext> GetActionContextAsync(this IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;

        // In .NET 10, IActionContextAccessor is obsolete, so we create ActionContext directly
        return GetActionContextAsync(httpContext);
    }

    public static async ValueTask<ActionContext> GetActionContextAsync(this HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue("OrchardCore:ActionContext", out var currentActionContext))
        {
            return (ActionContext)currentActionContext;
        }

        var endpoint = httpContext.GetEndpoint();
        var routeData = httpContext.GetRouteData();

        var actionDescriptor = endpoint?.Metadata.GetMetadata<ActionDescriptor>() ?? new ActionDescriptor();

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor );
        var filters = httpContext.RequestServices.GetServices<IAsyncViewActionFilter>();

        foreach (var filter in filters)
        {
            await filter.OnActionExecutionAsync(actionContext);
        }

        httpContext.Items["OrchardCore:ActionContext"] = actionContext;

        return actionContext;
    }
}
