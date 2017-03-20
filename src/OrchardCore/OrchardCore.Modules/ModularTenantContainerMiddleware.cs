using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Tenant;
using Orchard.Hosting;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware replaces the default service provider by the one for the current tenant
    /// </summary>
    public class ModularTenantContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantHost _orchardHost;
        private readonly IRunningTenantTable _runningTenantTable;
        private readonly ILogger _logger;

        public ModularTenantContainerMiddleware(
            RequestDelegate next,
            ITenantHost orchardHost,
            IRunningTenantTable runningTenantTable,
            ILogger<ModularTenantContainerMiddleware> logger)
        {
            _next = next;
            _orchardHost = orchardHost;
            _runningTenantTable = runningTenantTable;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Ensure all TenantContext are loaded and available.
            _orchardHost.Initialize();

            var tenantSetting = _runningTenantTable.Match(httpContext);

            // Register the tenant settings as a custom feature.
            httpContext.Features.Set(tenantSetting);

            // We only serve the next request if the tenant has been resolved.
            if (tenantSetting != null)
            {
                var tenantContext = _orchardHost.GetOrCreateTenantContext(tenantSetting);

                using (var scope = tenantContext.CreateServiceScope())
                {
                    httpContext.RequestServices = scope.ServiceProvider;

                    if (!tenantContext.IsActivated)
                    {
                        lock (tenantContext)
                        {
                            // The tenant gets activated here
                            if (!tenantContext.IsActivated)
                            {
                                var tenantEvents = scope.ServiceProvider
                                    .GetServices<IModularTenantEvents>();

                                foreach (var tenantEvent in tenantEvents)
                                {
                                    tenantEvent.ActivatingAsync().Wait();
                                }

                                httpContext.Items["BuildPipeline"] = true;
                                tenantContext.IsActivated = true;

                                foreach (var tenantEvent in tenantEvents)
                                {
                                    tenantEvent.ActivatedAsync().Wait();
                                }
                            }
                        }
                    }

                    await _next.Invoke(httpContext);
                }
            }
        }
    }
}