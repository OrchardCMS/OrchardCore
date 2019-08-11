using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Taxonomies.Drivers;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.GraphQL;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Liquid;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            // Registering both field types and shape types are necessary as they can 
            // be accessed from inner properties.

            TemplateContext.GlobalMemberAccessStrategy.Register<TaxonomyField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTaxonomyFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // Taxonomy Part
            services.AddScoped<IContentPartDisplayDriver, TaxonomyPartDisplayDriver>();
            services.AddSingleton<ContentPart, TaxonomyPart>();

            // Taxonomy Field
            services.AddSingleton<ContentField, TaxonomyField>();
            services.AddScoped<IContentFieldDisplayDriver, TaxonomyFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TaxonomyFieldIndexHandler>();

            services.AddScoped<IScopedIndexProvider, TaxonomyIndexProvider>();
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
