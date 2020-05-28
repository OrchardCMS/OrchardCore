using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Controllers;
using OrchardCore.Flows.Drivers;
using OrchardCore.Flows.Handlers;
using OrchardCore.Flows.Indexing;
using OrchardCore.Flows.Models;
using OrchardCore.Flows.Settings;
using OrchardCore.Flows.ViewModels;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Flows
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<BagPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FlowPartViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FlowMetadata>();
            TemplateContext.GlobalMemberAccessStrategy.Register<FlowPart>();
        }

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Flow Part
            services.AddContentPart<FlowPart>()
                .UseDisplayDriver<FlowPartDisplay>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, FlowPartSettingsDisplayDriver>();

            services.AddScoped<IContentDisplayDriver, FlowMetadataDisplay>();

            // Bag Part
            services.AddContentPart<BagPart>()
                .UseDisplayDriver<BagPartDisplay>()
                .AddHandler<BagPartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, BagPartSettingsDisplayDriver>();
            services.AddScoped<IContentPartIndexHandler, BagPartIndexHandler>();

            services.AddContentPart<FlowMetadata>();

            services.AddScoped<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Flows.BuildEditor",
                areaName: "OrchardCore.Flows",
                pattern: _adminOptions.AdminUrlPrefix + "/Flows/BuildEditor",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.BuildEditor) }
            );
        }
    }
}
