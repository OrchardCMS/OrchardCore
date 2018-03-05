using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.DeferredTasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Data;
using OrchardCore.Modules;
using OrchardCore.Mvc;
using OrchardCore.ResourceManagement;
using OrchardCore.ResourceManagement.TagHelpers;

namespace OrchardCore.Commons
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeferredTasks();
            services.AddDataAccess();
            services.AddResourceManagement();
            services.AddGeneratorTagFilter();
            services.AddCaching();
            services.AddShellDescriptorStorage();
            services.AddExtensionManager();
            services.AddTheming();
            services.AddLiquidViews();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            serviceProvider.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
            serviceProvider.AddTagHelpers(typeof(ShapeTagHelper).Assembly);
        }
    }

    /// <summary>
    /// Deferred tasks middleware is registered early as it has to run very late.
    /// </summary>
    public class DeferredTasksStartup : StartupBase
    {
        public override int Order => -50;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.AddDeferredTasks();
        }
    }
}
