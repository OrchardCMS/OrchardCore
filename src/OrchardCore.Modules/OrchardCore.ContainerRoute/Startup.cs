using System;
using System.Linq;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContainerRoute.Drivers;
using OrchardCore.ContainerRoute.Handlers;
using OrchardCore.ContainerRoute.Indexes;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContainerRoute.Settings;
using OrchardCore.ContainerRoute.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Routing;
using YesSql;

namespace OrchardCore.ContainerRoute
{
    public class Startup : StartupBase
    {
        public override int ConfigureOrder => -50;

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ContainerRoutePartViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // ContainerRoute Part
            services.AddContentPart<ContainerRoutePart>();
            services.AddScoped<IContentPartDisplayDriver, ContainerRoutePartDisplay>();
            //TODO Permissions
            //services.AddScoped<IPermissionProvider, Permissions>();

            services.AddScoped<IContentHandler, ContainerRouteContentHandler>();

            services.AddScoped<IContentPartHandler, ContainerRoutePartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContainerRoutePartSettingsDisplayDriver>();
            //services.AddScoped<IContentPartIndexHandler, ContainerRoutePartIndexHandler>();

            services.AddScoped<IScopedIndexProvider, ContainerRoutePartIndexProvider>();

            // Register the default route handler before the RouteHandlerPartHandler.
            services.AddScoped<IContentHandler, DefaultRouteHandler>();

            // RouteHandler Part
            services.AddContentPart<RouteHandlerPart>();
            services.AddScoped<IContentPartDisplayDriver, RouteHandlerPartDisplay>();

            services.AddScoped<IContentPartHandler, RouteHandlerPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, RouteHandlerPartSettingsDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddSingleton<IContainerRouteEntries, ContainerRouteEntries>();
            services.AddSingleton<IContentRouteProvider, ContainerRouteContentRouteProvider>();
            services.AddScoped<IContentRouteValidationProvider, ContainerRouteContentRouteValidationProvider>();

            services.AddSingleton<IShellRouteValuesAddressScheme, ContainerRouteValuesAddressScheme>();

            services.Configure<ContainerRouteOptions>(options =>
            {
                if (options.GlobalRouteValues.Count == 0)
                {
                    options.GlobalRouteValues = new RouteValueDictionary
                    {
                        {"Area", "OrchardCore.Contents"},
                        {"Controller", "Item"},
                        {"Action", "Display"}
                    };

                    options.ContainerContentItemIdKey = "contentItemId";
                    options.JsonPathKey = "jsonPath";
                    options.ContainedContentItemIdKey = "containedContentItemId";
                }
            });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var containerRouteEntries = serviceProvider.GetRequiredService<IContainerRouteEntries>();
            var session = serviceProvider.GetRequiredService<ISession>();

            var containerRoutes = session.QueryIndex<ContainerRoutePartIndex>(o => o.Published).ListAsync().GetAwaiter().GetResult();
            containerRouteEntries.AddEntries(containerRoutes.Select(x => new ContainerRouteEntry(x.ContainerContentItemId, x.Path, x.ContainedContentItemId, x.JsonPath)));
        }
    }
}
