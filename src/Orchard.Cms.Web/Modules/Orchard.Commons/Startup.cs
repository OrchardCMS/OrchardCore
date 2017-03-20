using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.BackgroundTasks;
using Orchard.Data;
using Orchard.DeferredTasks;
using Orchard.DisplayManagement;
using Orchard.Environment.Cache;
using OrchardCore.Extensions;
using OrchardCore.Tenant.Data;
using Orchard.ResourceManagement;

namespace Orchard.Commons
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddThemingHost();
            services.AddDeferredTasks();
            services.AddDataAccess();
            services.AddBackgroundTasks();
            services.AddResourceManagement();
            services.AddGeneratorTagFilter();
            services.AddCaching();
            services.AddTenantDescriptorStorage();
            services.AddExtensionManager();
            services.AddTheming();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // TODO: Order to be the late in the return pipeline
            app.AddDeferredTasks();
        }
    }
}
