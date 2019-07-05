using System;
using System.IO;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.Memory;

namespace OrchardCore.Media
{
    public class Startup : StartupBase
    {
        /// <summary>
        /// The request path used to route asset files
        /// </summary>
        public static readonly PathString AssetsRequestPath = new PathString("/media");

        /// <summary>
        /// The path in the tenant's App_Data folder containing the assets
        /// </summary>
        private const string AssetsPath = "Media";

        private readonly int _maxBrowserCacheDays;
        private readonly int _maxCacheDays;
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayMediaFieldViewModel>();
        }

        public Startup(IShellConfiguration shellConfiguration)
        {
            var configurationSection = shellConfiguration.GetSection("OrchardCore.Media");

            _maxBrowserCacheDays = configurationSection.GetValue("MaxBrowserCacheDays", 30);
            _maxCacheDays = configurationSection.GetValue("MaxCacheDays", 365);
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMediaFileProvider>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                var mediaPath = GetMediaPath(shellOptions.Value, shellSettings);

                if (!Directory.Exists(mediaPath))
                {
                    Directory.CreateDirectory(mediaPath);
                }
                return new MediaFileProvider(AssetsRequestPath, mediaPath);
            });

            services.AddSingleton<IStaticFileProvider>(serviceProvider =>
            {
                return serviceProvider.GetRequiredService<IMediaFileProvider>();
            });

            services.AddSingleton<IMediaFileStore>(serviceProvider =>
            {
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                var mediaPath = GetMediaPath(shellOptions.Value, shellSettings);
                var fileStore = new FileSystemStore(mediaPath);

                var mediaUrlBase = "/" + fileStore.Combine(shellSettings.RequestUrlPrefix, AssetsRequestPath);

                var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.Features.Get<ShellContextFeature>()?.OriginalPathBase ?? null;

                if (originalPathBase.HasValue)
                {
                    mediaUrlBase = fileStore.Combine(originalPathBase, mediaUrlBase);
                }

                return new MediaFileStore(fileStore, mediaUrlBase);
            });

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, AttachedMediaFieldsFolderAuthorizationHandler>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<ContentPart, ImageMediaPart>();
            services.AddMedia();

            services.AddLiquidFilter<MediaUrlFilter>("asset_url");
            services.AddLiquidFilter<ResizeUrlFilter>("resize_url");
            services.AddLiquidFilter<ImageTagFilter>("img_tag");

            // ImageSharp

            services.AddImageSharpCore(options =>
            {
                options.Configuration = Configuration.Default;
                options.MaxBrowserCacheDays = _maxBrowserCacheDays;
                options.MaxCacheDays = _maxCacheDays;
                options.CachedNameLength = 12;
                options.OnParseCommands = validation =>
                {
                    // Force some parameters to prevent disk filling.
                    // For more advanced resize parameters the usage of profiles will be necessary.
                    // This can be done with a custom IImageWebProcessor implementation that would 
                    // accept profile names.

                    validation.Commands.Remove(ResizeWebProcessor.Compand);
                    validation.Commands.Remove(ResizeWebProcessor.Sampler);
                    validation.Commands.Remove(ResizeWebProcessor.Xy);
                    validation.Commands.Remove(ResizeWebProcessor.Anchor);
                    validation.Commands.Remove(BackgroundColorWebProcessor.Color);

                    if (validation.Commands.Count > 0)
                    {
                        if (!validation.Commands.ContainsKey(ResizeWebProcessor.Mode))
                        {
                            validation.Commands[ResizeWebProcessor.Mode] = "max";
                        }
                    }
                };
                options.OnProcessed = _ => { };
                options.OnPrepareResponse = _ => { };
            })

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
            var mediaFileProvider = serviceProvider.GetRequiredService<IMediaFileProvider>();

            // ImageSharp before the static file provider
            app.UseImageSharp();

            app.UseStaticFiles(new StaticFileOptions
            {
                // The tenant's prefix is already implied by the infrastructure
                RequestPath = AssetsRequestPath,
                FileProvider = mediaFileProvider,
                ServeUnknownFileTypes = true,
            });
        }

        private string GetMediaPath(ShellOptions shellOptions, ShellSettings shellSettings)
        {
            return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, AssetsPath);
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
