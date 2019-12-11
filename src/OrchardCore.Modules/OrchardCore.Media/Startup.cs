using System;
using System.IO;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Liquid;
using OrchardCore.Media.Controllers;
using OrchardCore.Media.Core;
using OrchardCore.Media.Deployment;
using OrchardCore.Media.Drivers;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Filters;
using OrchardCore.Media.Handlers;
using OrchardCore.Media.Models;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Recipes;
using OrchardCore.Media.Services;
using OrchardCore.Media.Settings;
using OrchardCore.Media.TagHelpers;
using OrchardCore.Media.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.Memory;

namespace OrchardCore.Media
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayMediaFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<MediaOptions>, MediaOptionsConfiguration>();

            services.AddSingleton<IMediaFileProvider>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var options = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;

                var mediaPath = GetMediaPath(shellOptions.Value, shellSettings, options.AssetsPath);

                if (!Directory.Exists(mediaPath))
                {
                    Directory.CreateDirectory(mediaPath);
                }
                return new MediaFileProvider(options.AssetsRequestPath, mediaPath);
            });

            services.AddSingleton<IStaticFileProvider, IMediaFileProvider>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileProvider>()
            );

            services.AddSingleton<IMediaFileStore>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;

                var mediaPath = GetMediaPath(shellOptions.Value, shellSettings, mediaOptions.AssetsPath);
                var fileStore = new FileSystemStore(mediaPath);

                var mediaUrlBase = "/" + fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath);

                var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? null;

                if (originalPathBase.HasValue)
                {
                    mediaUrlBase = fileStore.Combine(originalPathBase.Value, mediaUrlBase);
                }

                return new DefaultMediaFileStore(fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl);
            });

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, AttachedMediaFieldsFolderAuthorizationHandler>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddContentPart<ImageMediaPart>();
            services.AddMedia();

            services.AddLiquidFilter<MediaUrlFilter>("asset_url");
            services.AddLiquidFilter<ResizeUrlFilter>("resize_url");
            services.AddLiquidFilter<ImageTagFilter>("img_tag");

            // ImageSharp

            // Add ImageSharp Configuration first, to override ImageSharp defaults.
            services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, MediaImageSharpConfiguration>();

            services.AddImageSharpCore()
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocator<ArrayPoolMemoryAllocator>()
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .AddProvider<MediaResizingFileProvider>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<ImageVersionProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

            // Media Field
            services.AddContentField<MediaField>();
            services.AddScoped<IContentFieldDisplayDriver, MediaFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MediaFieldSettingsDriver>();
            services.AddScoped<AttachedMediaFieldFileService, AttachedMediaFieldFileService>();
            services.AddScoped<IContentHandler, AttachedMediaFieldContentHandler>();
            services.AddScoped<IModularTenantEvents, TempDirCleanerService>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddRecipeExecutionStep<MediaStep>();

            // MIME types
            services.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddTagHelpers<ImageTagHelper>();
            services.AddTagHelpers<ImageResizeTagHelper>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var mediaFileProvider = serviceProvider.GetRequiredService<IMediaFileProvider>();
            var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
            var mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();

            // FileStore middleware before ImageSharp, but only if a remote storage module has registered a cache provider.
            if (mediaFileStoreCache != null)
            {
                app.UseMiddleware<MediaFileStoreResolverMiddleware>();
            }

            // ImageSharp before the static file provider.
            app.UseImageSharp();

            // Use the same cache control header as ImageSharp does for resized images.
            var cacheControl = "public, must-revalidate, max-age=" + TimeSpan.FromDays(mediaOptions.MaxBrowserCacheDays).TotalSeconds.ToString();

            app.UseStaticFiles(new StaticFileOptions
            {
                // The tenant's prefix is already implied by the infrastructure.
                RequestPath = mediaOptions.AssetsRequestPath,
                FileProvider = mediaFileProvider,
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = cacheControl;
                }
            });

            routes.MapAreaControllerRoute(
                name: "Media.Index",
                areaName: "OrchardCore.Media",
                pattern: _adminOptions.AdminUrlPrefix + "/Media",
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "MediaCache.Index",
                areaName: "OrchardCore.Media",
                pattern: _adminOptions.AdminUrlPrefix + "/MediaCache",
                defaults: new { controller = typeof(MediaCacheController).ControllerName(), action = nameof(MediaCacheController.Index) }
            );
        }

        private string GetMediaPath(ShellOptions shellOptions, ShellSettings shellSettings, string assetsPath)
        {
            return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, assetsPath);
        }
    }

    [Feature("OrchardCore.Media.Cache")]
    public class MediaCacheStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, MediaCachePermissions>();
            services.AddScoped<INavigationProvider, MediaCacheAdminMenu>();
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, MediaDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<MediaDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, MediaDeploymentStepDriver>();
        }
    }
}
