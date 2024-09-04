using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Media.Core;
using OrchardCore.Media.Deployment;
using OrchardCore.Media.Drivers;
using OrchardCore.Media.Events;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Filters;
using OrchardCore.Media.Handlers;
using OrchardCore.Media.Indexing;
using OrchardCore.Media.Liquid;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Recipes;
using OrchardCore.Media.Services;
using OrchardCore.Media.Settings;
using OrchardCore.Media.Shortcodes;
using OrchardCore.Media.TagHelpers;
using OrchardCore.Media.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;

namespace OrchardCore.Media;

public sealed class Startup : StartupBase
{
    public override int Order
        => OrchardCoreConstants.ConfigureOrder.Media;

    private const string ImageSharpCacheFolder = "is-cache";

    private readonly ShellSettings _shellSettings;

    public Startup(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<IAnchorTag, MediaAnchorTag>();

        // Resized media and remote media caches cleanups.
        services.AddSingleton<IBackgroundTask, ResizedMediaCacheBackgroundTask>();
        services.AddSingleton<IBackgroundTask, RemoteMediaCacheBackgroundTask>();

        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<DisplayMediaFieldViewModel>();
            o.MemberAccessStrategy.Register<Anchor>();

            o.Filters.AddFilter("img_tag", MediaFilters.ImgTag);
        })
        .AddLiquidFilter<AssetUrlFilter>("asset_url")
        .AddLiquidFilter<ResizeUrlFilter>("resize_url");

        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();

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
            var mediaEventHandlers = serviceProvider.GetServices<IMediaEventHandler>();
            var mediaCreatingEventHandlers = serviceProvider.GetServices<IMediaCreatingEventHandler>();
            var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStore>>();

            var mediaPath = GetMediaPath(shellOptions.Value, shellSettings, mediaOptions.AssetsPath);
            var fileStore = new FileSystemStore(mediaPath);

            var mediaUrlBase = "/" + fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath);

            var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext
                ?.Features.Get<ShellContextFeature>()
                ?.OriginalPathBase ?? PathString.Empty;

            if (originalPathBase.HasValue)
            {
                mediaUrlBase = fileStore.Combine(originalPathBase.Value, mediaUrlBase);
            }

            return new DefaultMediaFileStore(fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl, mediaEventHandlers, mediaCreatingEventHandlers, logger);
        });

        services.AddPermissionProvider<PermissionProvider>();
        services.AddScoped<IAuthorizationHandler, ManageMediaFolderAuthorizationHandler>();
        services.AddNavigationProvider<AdminMenu>();

        // ImageSharp

        // Add ImageSharp Configuration first, to override ImageSharp defaults.
        services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, MediaImageSharpConfiguration>();

        services
            .AddImageSharp()
            .RemoveProvider<PhysicalFileSystemProvider>()
            // For multitenancy we must use an absolute path to prevent leakage across tenants on different hosts.
            .SetCacheKey<BackwardsCompatibleCacheKey>()
            .Configure<PhysicalFileSystemCacheOptions>(options =>
            {
                options.CacheFolder = $"{_shellSettings.Name}/{ImageSharpCacheFolder}";
                options.CacheFolderDepth = 12;
            })
            .AddProvider<MediaResizingFileProvider>()
            .AddProcessor<ImageVersionProcessor>()
            .AddProcessor<TokenCommandProcessor>();

        services.AddScoped<MediaTokenSettingsUpdater>();
        services.AddSingleton<IMediaTokenService, MediaTokenService>();
        services.AddTransient<IConfigureOptions<MediaTokenOptions>, MediaTokenOptionsConfiguration>();
        services.AddScoped<IFeatureEventHandler>(sp => sp.GetRequiredService<MediaTokenSettingsUpdater>());
        services.AddScoped<IModularTenantEvents>(sp => sp.GetRequiredService<MediaTokenSettingsUpdater>());

        // Media Field
        services.AddContentField<MediaField>()
            .UseDisplayDriver<MediaFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, MediaFieldSettingsDriver>();
        services.AddScoped<AttachedMediaFieldFileService, AttachedMediaFieldFileService>();
        services.AddScoped<IContentHandler, AttachedMediaFieldContentHandler>();
        services.AddScoped<IModularTenantEvents, TempDirCleanerService>();
        services.AddDataMigration<Migrations>();
        services.AddRecipeExecutionStep<MediaStep>();

        // MIME types
        services.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

        services.AddTagHelpers<ImageTagHelper>();
        services.AddTagHelpers<ImageResizeTagHelper>();
        services.AddTagHelpers<AnchorTagHelper>();

        // Media Profiles
        services.AddScoped<MediaProfilesManager>();
        services.AddScoped<IMediaProfileService, MediaProfileService>();
        services.AddRecipeExecutionStep<MediaProfileStep>();

        // Media Name Normalizer
        services.AddScoped<IMediaNameNormalizerService, NullMediaNameNormalizerService>();

        services.AddScoped<IUserAssetFolderNameProvider, DefaultUserAssetFolderNameProvider>();
        services.AddSingleton<IChunkFileUploadService, ChunkFileUploadService>();
        services.AddSingleton<IBackgroundTask, ChunkFileUploadBackgroundTask>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var mediaFileProvider = serviceProvider.GetRequiredService<IMediaFileProvider>();
        var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
        var mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();

        // Move middleware into SecureMediaStartup if it is possible to insert it between the users and media
        // module. See issue https://github.com/OrchardCMS/OrchardCore/issues/15716.
        // Secure media file middleware, but only if the feature is enabled.
        if (serviceProvider.IsSecureMediaEnabled())
        {
            app.UseMiddleware<SecureMediaMiddleware>();
        }

        // FileStore middleware before ImageSharp, but only if a remote storage module has registered a cache provider.
        if (mediaFileStoreCache != null)
        {
            app.UseMiddleware<MediaFileStoreResolverMiddleware>();
        }

        // ImageSharp before the static file provider.
        app.UseImageSharp();

        // The file provider is a circular dependency and replaceable via di.
        mediaOptions.StaticFileOptions.FileProvider = mediaFileProvider;

        // Use services.PostConfigure<MediaOptions>() to alter the media static file options event handlers.
        app.UseStaticFiles(mediaOptions.StaticFileOptions);
    }

    private static string GetMediaPath(ShellOptions shellOptions, ShellSettings shellSettings, string assetsPath)
    {
        return PathExtensions.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, assetsPath);
    }
}

[Feature("OrchardCore.Media.Cache")]
public sealed class MediaCacheStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<MediaCachePermissions>();
        services.AddNavigationProvider<MediaCacheAdminMenu>();
    }
}

[Feature("OrchardCore.Media.Slugify")]
public sealed class MediaSlugifyStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Media Name Normalizer
        services.AddScoped<IMediaNameNormalizerService, SlugifyMediaNameNormalizerService>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<MediaDeploymentSource, MediaDeploymentStep, MediaDeploymentStepDriver>();
        services.AddDeployment<AllMediaProfilesDeploymentSource, AllMediaProfilesDeploymentStep, AllMediaProfilesDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.Media.Indexing")]
public sealed class MediaIndexingStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentFieldIndexHandler, MediaFieldIndexHandler>();
    }
}

[Feature("OrchardCore.Media.Indexing.Text")]
public sealed class TextIndexingStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMediaFileTextProvider<TextMediaFileTextProvider>(".txt");
        services.AddMediaFileTextProvider<TextMediaFileTextProvider>(".md");
    }
}

[RequireFeatures("OrchardCore.Shortcodes")]
public sealed class ShortcodesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Only add image as a descriptor as [media] is deprecated.
        services.AddShortcode<ImageShortcodeProvider>("image", d =>
        {
            d.DefaultValue = "[image] [/image]";
            d.Hint = "Add a image from the media library.";
            d.Usage =
@"[image]foo.jpg[/image]<br>
<table>
  <tr>
    <td>Args:</td>
    <td>width, height, mode</td>
  </tr>
  <tr>
    <td></td>
    <td>class, alt</td>
  </tr>
</table>";
            d.Categories = ["HTML Content", "Media"];
        });

        services.AddShortcode<AssetUrlShortcodeProvider>("asset_url", d =>
        {
            d.DefaultValue = "[asset_url] [/asset_url]";
            d.Hint = "Return a url from the media library.";
            d.Usage =
@"[asset_url]foo.jpg[/asset_url]<br>
<table>
  <tr>
    <td>Args:</td>
    <td>width, height, mode</td>
  </tr>
</table>";
            d.Categories = ["HTML Content", "Media"];
        });
    }
}

[Feature("OrchardCore.Media.Security")]
public sealed class SecureMediaStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Marker service to easily detect if the feature has been enabled.
        services.AddSingleton<SecureMediaMarker>();
        services.AddPermissionProvider<SecureMediaPermissions>();
        services.AddScoped<IAuthorizationHandler, ViewMediaFolderAuthorizationHandler>();

        services.AddSingleton<IMediaEventHandler, SecureMediaFileStoreEventHandler>();
    }
}
