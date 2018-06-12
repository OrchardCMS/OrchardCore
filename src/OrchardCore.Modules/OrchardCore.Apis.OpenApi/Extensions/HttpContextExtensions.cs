using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis.OpenApi
{
    internal static class HttpContextExtensions
    {
        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public static Task WriteModelAsync<TModel>(this HttpContext context, TModel model)
        {
            var result = new ObjectResult(model)
            {
                DeclaredType = typeof(TModel)
            };

            var executor = context.RequestServices.GetRequiredService<ObjectResultExecutor>();

            var routeData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
