using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tenant;
using Orchard.Hosting;

namespace Orchard.DeferredTasks
{
    /// <summary>
    /// Executes any deferred tasks when the request is terminated.
    /// </summary>
    public class DeferredTaskMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantHost _orchardHost;

        public DeferredTaskMiddleware(RequestDelegate next, ITenantHost orchardHost)
        {
            _next = next;
            _orchardHost = orchardHost;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);

            // Register the tenant settings as a custom feature.
            var tenantSettings = httpContext.Features.Get<TenantSettings>();

            // We only serve the next request if the tenant has been resolved.
            if (tenantSettings != null)
            {
                var tenantContext = _orchardHost.GetOrCreateTenantContext(tenantSettings);

                using (var scope = tenantContext.CreateServiceScope())
                {
                    var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                    if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                    {
                        var context = new DeferredTaskContext(scope.ServiceProvider);
                        await deferredTaskEngine.ExecuteTasksAsync(context);
                    }
                }
            }
        }
    }
}
