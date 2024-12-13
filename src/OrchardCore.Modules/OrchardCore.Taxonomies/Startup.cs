using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using OrchardCore.Taxonomies.Drivers;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.GraphQL;
using OrchardCore.Taxonomies.Handlers;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Liquid;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Services;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<TaxonomyField>();
            o.MemberAccessStrategy.Register<TaxonomyPartViewModel>();
            o.MemberAccessStrategy.Register<TermPartViewModel>();
            o.MemberAccessStrategy.Register<DisplayTaxonomyFieldViewModel>();
            o.MemberAccessStrategy.Register<DisplayTaxonomyFieldTagsViewModel>();
        })
        .AddLiquidFilter<InheritedTermsFilter>("inherited_terms")
        .AddLiquidFilter<TaxonomyTermsFilter>("taxonomy_terms");

        services.AddDataMigration<Migrations>();
        services.AddShapeTableProvider<TermShapes>();
        services.AddPermissionProvider<Permissions>();

        // Taxonomy Part
        services.AddContentPart<TaxonomyPart>()
            .UseDisplayDriver<TaxonomyPartDisplayDriver>()
            .AddHandler<TaxonomyPartHandler>();

        // Taxonomy Field
        services.AddContentField<TaxonomyField>()
            .UseDisplayDriver<TaxonomyFieldDisplayDriver>(d => !string.Equals(d, "Tags", StringComparison.OrdinalIgnoreCase))
            .AddHandler<TaxonomyFieldHandler>();

        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldSettingsDriver>();
        services.AddScoped<IContentFieldIndexHandler, TaxonomyFieldIndexHandler>();

        // Taxonomy Tags Display Mode and Editor.
        services.AddContentField<TaxonomyField>()
            .UseDisplayDriver<TaxonomyFieldTagsDisplayDriver>(d => string.Equals(d, "Tags", StringComparison.OrdinalIgnoreCase));

        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldTagsEditorSettingsDriver>();

        services.AddScopedIndexProvider<TaxonomyIndexProvider>();

        // Terms.
        services.AddContentPart<TermPart>();
        services.AddScoped<IContentHandler, TermPartContentHandler>();
        services.AddScoped<IContentDisplayDriver, TermPartContentDriver>();
    }
}

[Feature("OrchardCore.Taxonomies.ContentsAdminList")]
public sealed class ContentsAdminListStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentsAdminListFilter, TaxonomyContentsAdminListFilter>();
        services.AddDisplayDriver<ContentOptionsViewModel, TaxonomyContentsAdminListDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddSiteDisplayDriver<TaxonomyContentsAdminListSettingsDisplayDriver>();
    }
}

[Feature("OrchardCore.Taxonomies.ContentsAdminList")]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class ContentsAdminListDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<TaxonomyContentsAdminListSettings, ContentsAdminListDeploymentStartup>(S => S["Taxonomy Filters settings"], S => S["Exports the Taxonomy filters settings."]);
    }
}

[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class GraphQLStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectGraphType<TaxonomyPart, TaxonomyPartQueryObjectType>();
        services.AddObjectGraphType<TaxonomyField, TaxonomyFieldQueryObjectType>();
    }
}
