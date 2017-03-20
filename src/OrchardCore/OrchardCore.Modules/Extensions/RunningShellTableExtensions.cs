using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.Tenant;

namespace OrchardCore.Modules
{
    public static class RunningTenantTableExtensions
    {
        public static TenantSettings Match(this IRunningTenantTable table, HttpContext httpContext)
        {
            // use Host header to prevent proxy alteration of the orignal request
            try
            {
                var httpRequest = httpContext.Request;
                if (httpRequest == null)
                {
                    return null;
                }

                var host = httpRequest.Headers["Host"].ToString();

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