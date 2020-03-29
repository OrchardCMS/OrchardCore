using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Security.Permissions;
using OrchardCore.Taxonomies.Controllers;
using OrchardCore.Taxonomies.Drivers;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.GraphQL;
using OrchardCore.Taxonomies.Handlers;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Liquid;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            // Registering both field types and shape types are necessary as they can
            // be accessed from inner properties.

            TemplateContext.GlobalMemberAccessStrategy.Register<TaxonomyField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTaxonomyFieldViewModel>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTaxonomyFieldTagsViewModel>();
        }

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IShapeTableProvider, TermShapes>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // Taxonomy Part
            services.AddContentPart<TaxonomyPart>()
                .UseDisplayDriver<TaxonomyPartDisplayDriver>()
                .AddHandler<TaxonomyPartHandler>();

            // Taxonomy Field
            services.AddContentField<TaxonomyField>()
                .UseDisplayDriver<TaxonomyFieldDisplayDriver>(d => !String.Equals(d, "Tags", StringComparison.OrdinalIgnoreCase));

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TaxonomyFieldIndexHandler>();

            // Taxonomy Tags Display Mode and Editor.
            services.AddContentField<TaxonomyField>()
                .UseDisplayDriver<TaxonomyFieldTagsDisplayDriver>(d => String.Equals(d, "Tags", StringComparison.OrdinalIgnoreCase));

            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldTagsEditorSettingsDriver>();

            services.AddScoped<IScopedIndexProvider, TaxonomyIndexProvider>();

            // Terms
            services.AddContentPart<TermPart>();
            services.AddScoped<IContentHandler, TermPartContentHandler>();
            services.AddScoped<IContentDisplayDriver, TermPartContentDriver>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var taxonomyControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Taxonomies.Create",
                areaName: "OrchardCore.Taxonomies",
                pattern: _adminOptions.AdminUrlPrefix + "/Taxonomies/Create/{id}",
                defaults: new { controller = taxonomyControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Taxonomies.Edit",
                areaName: "OrchardCore.Taxonomies",
                pattern: _adminOptions.AdminUrlPrefix + "/Taxonomies/Edit/{taxonomyContentItemId}/{taxonomyItemId}",
                defaults: new { controller = taxonomyControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "Taxonomies.Delete",
                areaName: "OrchardCore.Taxonomies",
                pattern: _adminOptions.AdminUrlPrefix + "/Taxonomies/Delete/{taxonomyContentItemId}/{taxonomyItemId}",
                defaults: new { controller = taxonomyControllerName, action = nameof(AdminController.Delete) }
            );
        }
    }

    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddLiquidFilter<TaxonomyTermsFilter>("taxonomy_terms");
            services.AddLiquidFilter<InheritedTermsFilter>("inherited_terms");
        }
    }

    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class GraphQLStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<TaxonomyPart, TaxonomyPartQueryObjectType>();
            services.AddObjectGraphType<TaxonomyField, TaxonomyFieldQueryObjectType>();
        }
    }
}
