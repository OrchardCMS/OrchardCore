using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    public static class RunningShellTableExtensions
    {
        private const string XForwardedHost = "X-Forwarded-Host";

        public static ShellSettings Match(this IRunningShellTable table, HttpContext httpContext)
        {
            var httpRequest = httpContext.Request;

            if (httpRequest == null)
            {
                return null;
            }

            // The Host property contains the value as set from the client. It is replaced automatically
            // to the value of X-Forwarded-Host when UseIISIntegration() is invoked.
            // The same way .Scheme contains the protocol that the user set and not what a proxy
            // could be using (see X-Forwarded-Proto).

            return table.Match(httpRequest.Host.ToString(), httpRequest.Path);
        }
    }
}