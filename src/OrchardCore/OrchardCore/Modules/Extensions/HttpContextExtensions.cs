using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules
{
    internal static class HttpContextExtensions
    {
        /// <summary>
        /// Makes <see cref="HttpContext.RequestServices"/> aware of the current <see cref="ShellScope"/>.
        /// </summary>
        public static HttpContext UseShellScopeServices(this HttpContext httpContext)
        {
            httpContext.RequestServices = new ShellScopeServices(httpContext.RequestServices);
            return httpContext;
        }

        /// <summary>
        /// Registers the <see cref="ShellContext"/> to be available for the full request.
        /// E.g for logging purposes when an exception is catched outside any shell scope.
        /// </summary>
        public static void RegisterShellContext(this HttpContext httpContext, ShellContext shellContext)
            => httpContext.Features.Set<ShellContext>(shellContext);

        /// <summary>
        /// Invokes a <see cref="Delegate"/> as a <see cref="RequestDelegate"/>
        /// </summary>
        public static Task Invoke(this Delegate pipeline, HttpContext httpContext)
            => (pipeline as RequestDelegate).Invoke(httpContext);
    }
}
