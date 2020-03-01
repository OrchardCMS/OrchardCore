using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Contents.AdminNodes;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Deployment;
using OrchardCore.Contents.Drivers;
using OrchardCore.Contents.Feeds.Builders;
using OrchardCore.Contents.Handlers;
using OrchardCore.Contents.Indexing;
using OrchardCore.Contents.Liquid;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Recipes;
using OrchardCore.Contents.Security;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.Settings;
using OrchardCore.Contents.Sitemaps;
using OrchardCore.Contents.TagHelpers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Feeds;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Lists.Settings;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Contents
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentManagement();
            services.AddContentManagementDisplay();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IPermissionProvider, ContentTypePermissions>();
            services.AddScoped<IAuthorizationHandler, ContentTypeAuthorizationHandler>();
            services.AddScoped<IShapeTableProvider, Shapes>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentDisplayDriver, ContentsDriver>();
            services.AddScoped<IContentHandler, ContentsHandler>();
            services.AddRecipeExecutionStep<ContentStep>();

            services.AddScoped<IContentItemIndexHandler, FullTextContentIndexHandler>();
            services.AddScoped<IContentItemIndexHandler, AspectsContentIndexHandler>();
            services.AddScoped<IContentItemIndexHandler, DefaultContentIndexHandler>();
            services.AddScoped<IContentAliasProvider, ContentItemIdAliasProvider>();
            services.AddScoped<IContentItemIndexHandler, ContentItemIndexCoordinator>();

            services.AddIdGeneration();
            services.AddScoped<IDataMigration, Migrations>();

            // Common Part
            services.AddContentPart<CommonPart>()
                .UseDisplayDriver<DateEditorDriver>()
                .UseDisplayDriver<OwnerEditorDriver>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, CommonPartSettingsDisplayDriver>();

            // FullTextAspect
            services.AddScoped<IContentTypeDefinitionDisplayDriver, FullTextAspectSettingsDisplayDriver>();
            services.AddScoped<IContentHandler, FullTextAspectSettingsHandler>();

            // Feeds
            // TODO: Move to feature
            services.AddScoped<IFeedItemBuilder, CommonFeedItemBuilder>();

            services.AddTagHelpers<ContentLinkTagHelper>();
            services.AddTagHelpers<ContentItemTagHelper>();
            services.Configure<AutorouteOptions>(options =>
            {
                if (options.GlobalRouteValues.Count == 0)
                {
                    options.GlobalRouteValues = new RouteValueDictionary
                    {
                        {"Area", "OrchardCore.Contents"},
                        {"Controller", "Item"},
                        {"Action", "Display"}
                    };

                    options.ContentItemIdKey = "contentItemId";
                    options.ContainedContentItemIdKey = "containedContentItemId";
                    options.JsonPathKey = "jsonPath";
                }
            });
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var itemControllerName = typeof(ItemController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "DisplayContentItem",
                areaName: "OrchardCore.Contents",
                pattern: "Contents/ContentItems/{contentItemId}",
                defaults: new { controller = itemControllerName, action = nameof(ItemController.Display) }
            );

            routes.MapAreaControllerRoute(
                name: "PreviewContentItem",
                areaName: "OrchardCore.Contents",
                pattern: "Contents/ContentItems/{contentItemId}/Preview",
                defaults: new { controller = itemControllerName, action = nameof(ItemController.Preview) }
            );

            routes.MapAreaControllerRoute(
                name: "PreviewContentItemVersion",
                areaName: "OrchardCore.Contents",
                pattern: "Contents/ContentItems/{contentItemId}/Version/{version}/Preview",
                defaults: new { controller = itemControllerName, action = nameof(ItemController.Preview) }
            );

            // Admin
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "EditContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/Edit",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "CreateContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentTypes/{id}/Create",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/Display",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Display) }
            );

            routes.MapAreaControllerRoute(
                name: "ListContentItems",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentTypeId?}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.List) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminPublishContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/Publish",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Publish) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminDiscardDraftContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/DiscardDraft",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.DiscardDraft) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminDeleteContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/Delete",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Remove) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminCloneContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/Clone",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Clone) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminUnpublishContentItem",
                areaName: "OrchardCore.Contents",
                pattern: _adminOptions.AdminUrlPrefix + "/Contents/ContentItems/{contentItemId}/Unpublish",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Unpublish) }
            );
        }
    }

    [RequireFeatures("OrchardCore.Liquid")]
    public class LiquidStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ILiquidTemplateEventHandler, ContentLiquidTemplateEventHandler>();

            services.AddLiquidFilter<BuildDisplayFilter>("shape_build_display");
            services.AddLiquidFilter<ContentItemFilter>("content_item_id");
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, AllContentDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllContentDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllContentDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, ContentDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ContentDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ContentDeploymentStepDriver>();
        }
    }

    [RequireFeatures("OrchardCore.AdminMenu")]
    public class AdminMenuStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<ContentTypesAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, ContentTypesAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, ContentTypesAdminNodeDriver>();
        }
    }

    [Feature("OrchardCore.Contents.FileContentDefinition")]
    public class FileContentDefinitionStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddFileContentDefinitionStore();
        }
    }

    [RequireFeatures("OrchardCore.Sitemaps")]
    public class SitemapsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISitemapSourceBuilder, ContentTypesSitemapSourceBuilder>();
            services.AddScoped<ISitemapSourceCacheManager, ContentTypesSitemapSourceCacheManager>();
            services.AddScoped<ISitemapSourceModifiedDateProvider, ContentTypesSitemapSourceModifiedDateProvider>();
            services.AddScoped<IDisplayDriver<SitemapSource>, ContentTypesSitemapSourceDriver>();
            services.AddScoped<ISitemapSourceFactory, SitemapSourceFactory<ContentTypesSitemapSource>>();
            services.AddScoped<IContentItemsQueryProvider, DefaultContentItemsQueryProvider>();
            services.AddScoped<IContentHandler, ContentTypesSitemapCacheHandler>();
        }
    }
}
