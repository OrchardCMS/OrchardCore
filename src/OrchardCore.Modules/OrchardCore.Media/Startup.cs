using System;
using System.IO;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Liquid;
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
        private readonly IShellConfiguration _shellConfiguration;

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayMediaFieldViewModel>();
        }

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
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

            // Register with strong service descriptor to facilitate removal by Media.Azure.
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IStaticFileProvider, IMediaFileProvider>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileProvider>()
            ));

            services.AddSingleton<IMediaFileStorePathProvider, MediaFileStorePathProvider>();

            services.AddSingleton<IMediaFileStore>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var mediaFileStoreOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>();
                var pathProvider = serviceProvider.GetRequiredService<IMediaFileStorePathProvider>();

                var mediaPath = GetMediaPath(shellOptions.Value, shellSettings, mediaFileStoreOptions.Value.AssetsPath);
                var fileStore = new FileSystemStore(mediaPath);

                return new MediaFileStore(fileStore, pathProvider, mediaFileStoreOptions);
            });

            services.TryAddEnumerable(ServiceDescriptor.Singleton<ICdnPathProvider, IMediaFileStorePathProvider>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileStorePathProvider>()));

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, AttachedMediaFieldsFolderAuthorizationHandler>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<ContentPart, ImageMediaPart>();
            services.AddMedia();

            services.AddLiquidFilter<MediaUrlFilter>("asset_url");
            services.AddLiquidFilter<ResizeUrlFilter>("resize_url");
            services.AddLiquidFilter<ImageTagFilter>("img_tag");

            // ImageSharp

            // Add ImageSharp Configuration first, to override ImageSharp defaults.
            services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, MediaImageSharpConfiguration>();

            //TODO consider.
            services.AddSingleton<IMediaImageCache, MediaFileCache>();

            services.AddImageSharpCore()
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocator<ArrayPoolMemoryAllocator>()
                .SetCache(serviceProvider => serviceProvider.GetRequiredService<IMediaImageCache>())
                .SetCacheHash<CacheHash>()
                .AddProvider<MediaResizingFileProvider>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<ImageVersionProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

            // Media Field
            services.AddSingleton<ContentField, MediaField>();
            services.AddScoped<IContentFieldDisplayDriver, MediaFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MediaFieldSettingsDriver>();
            services.AddScoped<AttachedMediaFieldFileService, AttachedMediaFieldFileService>();
            services.AddScoped<IContentHandler, AttachedMediaFieldContentHandler>();
            services.AddScoped<IModularTenantEvents, TempDirCleanerService>();

            services.AddRecipeExecutionStep<MediaStep>();

            // MIME types
            services.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddTagHelpers<ImageTagHelper>();
            services.AddTagHelpers<ImageResizeTagHelper>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var mediaFileProvider = serviceProvider.GetService<IMediaFileProvider>();

            var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>();

            var mediaCacheFileProvider = serviceProvider.GetService<IMediaCacheFileProvider>();

            // FileStore middleware before ImageSharp, but only if a remote storage module has registered it.
            if (mediaCacheFileProvider != null)
            {
                app.UseMiddleware<MediaFileStoreResolverMiddleware>();
            }

            // ImageSharp before the static file provider.
            app.UseImageSharp();

            app.UseStaticFiles(new StaticFileOptions
            {
                // The tenant's prefix is already implied by the infrastructure.
                RequestPath = mediaOptions.Value.AssetsRequestPath,
                FileProvider = mediaFileProvider,
                ServeUnknownFileTypes = true,
            });
        }

        private string GetMediaPath(ShellOptions shellOptions, ShellSettings shellSettings, string assetsPath)
        {
            return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, assetsPath);
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
