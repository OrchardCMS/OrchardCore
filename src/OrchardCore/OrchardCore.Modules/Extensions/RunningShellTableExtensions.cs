using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    public static class RunningShellTableExtensions
    {
        private static string XForwardedHost = "X-Forwarded-Host";

        public static ShellSettings Match(this IRunningShellTable table, HttpContext httpContext)
        {
            // use Host header to prevent proxy alteration of the orignal request
            try
            {
                var httpRequest = httpContext.Request;

                if (httpRequest == null)
                {
                    return null;
                }

                string host;

                // If the "X-Forwarded-Host" header is set, use it as the host that the client requested. 
                if (httpRequest.Headers.TryGetValue(XForwardedHost, out var forwardedHost) && forwardedHost.Count > 0)
                {
                    host = forwardedHost[0];
                }
                else
                {
                    host = httpRequest.Host.ToString();
                }

                return table.Match(host ?? string.Empty, httpRequest.Path);
            }
            catch (Exception)
            {
                // can happen on cloud service for an unknown reason
                return null;
            }
        }
    }
}