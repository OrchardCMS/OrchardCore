using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DeferredTasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        public static TenantServicesBuilder AddDeferredTasks(this TenantServicesBuilder tenant)
        {
            var services = tenant.Services;

            services.TryAddScoped<IDeferredTaskEngine, DeferredTaskEngine>();
            services.TryAddScoped<IDeferredTaskState, HttpContextTaskState>();

            return tenant;
        }
    }
}