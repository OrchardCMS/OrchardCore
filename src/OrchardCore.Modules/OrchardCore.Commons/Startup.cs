using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Modules;
using OrchardCore.Mvc;
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
            services.AddGeneratorTagFilter();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            serviceProvider.AddTagHelpers(typeof(ResourcesTagHelper).Assembly);
            serviceProvider.AddTagHelpers(typeof(ShapeTagHelper).Assembly);
        }
    }
}
