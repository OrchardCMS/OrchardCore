using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Handles a request by forwarding it to the tenant specific pipeline.
    /// It also initializes the middlewares for the requested tenant on the first request.
    /// </summary>
    public class ModularTenantRouterMiddleware
    {
        private readonly TenantPipelineInitializer _tenantPipelineInitializer;
        private readonly IFeatureCollection _features;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

        public ModularTenantRouterMiddleware(
            RequestDelegate _,
            TenantPipelineInitializer tenantPipelineInitializer,
            IFeatureCollection features,
            ILogger<ModularTenantRouterMiddleware> logger)
        {
            _tenantPipelineInitializer = tenantPipelineInitializer;
            _features = features;
            _logger = logger;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Begin Routing Request");
            }

            return _tenantPipelineInitializer.InitializeAsync(httpContext, ShellScope.Context, _features);
        }
    }
}
