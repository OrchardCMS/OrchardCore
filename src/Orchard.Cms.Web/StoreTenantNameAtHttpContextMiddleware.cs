using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using System.Threading.Tasks;

namespace Orchard.Cms.Web
{
    /// <summary>
    /// This middleware collect data useful for other services in httpContext 
    /// </summary>
    public class StoreTenantNameAtHttpContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRunningShellTable _runningShellTable;

        public StoreTenantNameAtHttpContextMiddleware(
            RequestDelegate next,
            IRunningShellTable runningShellTable)
        {
            _next = next;
            _runningShellTable = runningShellTable;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var shellSetting = _runningShellTable.Match(httpContext);
            if (shellSetting != null)
            {                
                httpContext.Items["tenant"] = shellSetting.Name;                
            }
            await _next.Invoke(httpContext);

        }
    }
}