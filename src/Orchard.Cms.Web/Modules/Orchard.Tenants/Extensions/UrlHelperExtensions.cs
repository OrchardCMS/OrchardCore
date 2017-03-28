using Microsoft.AspNetCore.Mvc;
using Orchard.Environment.Shell;
using System;
using System.Linq;

namespace Orchard.Tenants.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string Tenant(this IUrlHelper urlHelper, ShellSettings tenantShellSettings)
        {
            var requestHostInfo = urlHelper.ActionContext.HttpContext.Request.Host;
            var requestScheme = urlHelper.ActionContext.HttpContext.Request.Scheme;

            var tenantUrlHost = tenantShellSettings.RequestUrlHost?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).First() ?? requestHostInfo.Host;
            if(requestHostInfo.Port.HasValue)
            {
                tenantUrlHost += ":" + requestHostInfo.Port;
            }

            var result = $"{requestScheme}://{tenantUrlHost}";

            if (!string.IsNullOrEmpty(tenantShellSettings.RequestUrlPrefix))
            {
                result += "/" + tenantShellSettings.RequestUrlPrefix;
            }

            return result;
        }
    }
}
