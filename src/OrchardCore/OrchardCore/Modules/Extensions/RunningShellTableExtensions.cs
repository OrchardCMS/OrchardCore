using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    // Not public because it wouldn't match tenants with an URL prefix later in the request pipeline. Mostly to be used
    // from ModularTenantContainerMiddleware.
    // For details see: https://github.com/OrchardCMS/OrchardCore/pull/10779#discussion_r758741155.
    internal static class RunningShellTableExtensions
    {
        public static ShellSettings Match(this IRunningShellTable table, HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var httpRequest = httpContext.Request;

            // The Host property contains the value as set from the client. It is replaced automatically
            // to the value of X-Forwarded-Host when UseIISIntegration() is invoked.
            // The same way .Scheme contains the protocol that the user set and not what a proxy
            // could be using (see X-Forwarded-Proto).

            return table.Match(httpRequest.Host, httpRequest.Path, true);
        }
    }
}
