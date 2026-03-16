using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
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
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Media.Controllers;
using OrchardCore.Media.Core;
using OrchardCore.Media.Deployment;
using OrchardCore.Media.Drivers;
using OrchardCore.Media.Events;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Filters;
using OrchardCore.Media.Handlers;
using OrchardCore.Media.Hubs;
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
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;

namespace OrchardCore.Media;

public sealed class Startup : StartupBase
{
    public override int Order => OrchardCoreConstants.ConfigureOrder.Media;

    private const string ImageSharpCacheFolder = "is-cache";

    private readonly ShellSettings _shellSettings;

    public Startup(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddSingleton<IJSLocalizer, MediaJSLocalizer>();
        services.AddSingleton<IJSLocalizer, NullJSLocalizer>();
        services.AddSingleton<IAnchorTag, MediaAnchorTag>();

        // In-memory directory tree cache (built lazily, invalidated on folder mutations).
        services.AddSingleton<MediaDirectoryTreeCache>();

        // Resized media and remote media caches cleanups.
        services.AddSingleton<IBackgroundTask, ResizedMediaCacheBackgroundTask>();
        services.AddSingleton<IBackgroundTask, RemoteMediaCacheBackgroundTask>();

        services
            .Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<DisplayMediaFieldViewModel>();
                o.MemberAccessStrategy.Register<Anchor>();

                o.Filters.AddFilter("img_tag", MediaFilters.ImgTag);
            })
            .AddLiquidFilter<AssetUrlFilter>("asset_url")
            .AddLiquidFilter<ResizeUrlFilter>("resize_url");

        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

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
            var mediaCreatingEventHandlers =
                serviceProvider.GetServices<IMediaCreatingEventHandler>();
            var fileSystemStoreLogger = serviceProvider.GetRequiredService<
                ILogger<FileSystemStore>
            >();
            var defaultMediaFileStoreLogger = serviceProvider.GetRequiredService<
                ILogger<DefaultMediaFileStore>
            >();

            var mediaPath = GetMediaPath(
                shellOptions.Value,
                shellSettings,
                mediaOptions.AssetsPath
            );
            var fileStore = new FileSystemStore(mediaPath, fileSystemStoreLogger);

            var mediaUrlBase =
                "/"
                + fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath);

            var originalPathBase =
                serviceProvider
                    .GetRequiredService<IHttpContextAccessor>()
                    .HttpContext?.Features.Get<ShellContextFeature>()
                    ?.OriginalPathBase
                ?? PathString.Empty;

            if (originalPathBase.HasValue)
            {
                mediaUrlBase = fileStore.Combine(originalPathBase.Value, mediaUrlBase);
            }

            return new DefaultMediaFileStore(
                fileStore,
                mediaUrlBase,
                mediaOptions.CdnBaseUrl,
                mediaEventHandlers,
                mediaCreatingEventHandlers,
                defaultMediaFileStoreLogger
            );
        });

        services.AddPermissionProvider<PermissionProvider>();
        services.AddScoped<IAuthorizationHandler, ManageMediaFolderAuthorizationHandler>();
        services.AddNavigationProvider<AdminMenu>();

        // ImageSharp

        // Add ImageSharp Configuration first, to override ImageSharp defaults.
        services.AddTransient<
            IConfigureOptions<ImageSharpMiddlewareOptions>,
            MediaImageSharpConfiguration
        >();

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
        services.AddTransient<
            IConfigureOptions<MediaTokenOptions>,
            MediaTokenOptionsConfiguration
        >();
        services.AddScoped<IFeatureEventHandler>(sp =>
            sp.GetRequiredService<MediaTokenSettingsUpdater>()
        );
        services.AddScoped<IModularTenantEvents>(sp =>
            sp.GetRequiredService<MediaTokenSettingsUpdater>()
        );

        // Media Field
        services.AddContentField<MediaField>().UseDisplayDriver<MediaFieldDisplayDriver>();
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

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider
    )
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

    private static string GetMediaPath(
        ShellOptions shellOptions,
        ShellSettings shellSettings,
        string assetsPath
    )
    {
        return PathExtensions.Combine(
            shellOptions.ShellsApplicationDataPath,
            shellOptions.ShellsContainerName,
            shellSettings.Name,
            assetsPath
        );
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
        services.AddDeployment<
            MediaDeploymentSource,
            MediaDeploymentStep,
            MediaDeploymentStepDriver
        >();
        services.AddDeployment<
            AllMediaProfilesDeploymentSource,
            AllMediaProfilesDeploymentStep,
            AllMediaProfilesDeploymentStepDriver
        >();
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
        services.AddShortcode<ImageShortcodeProvider>(
            "image",
            d =>
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
            }
        );

        services.AddShortcode<AssetUrlShortcodeProvider>(
            "asset_url",
            d =>
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
            }
        );
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

[Feature("OrchardCore.Media.Tus")]
public sealed class MediaTusStartup : StartupBase
{
    // Run very early so the size-limit middleware executes before anything
    // else in the tenant pipeline can attempt to read the request body.
    public override int Order => OrchardCoreConstants.ConfigureOrder.ReverseProxy - 10;

    public override void ConfigureServices(IServiceCollection services)
    {
        // Marker service so views/controllers can detect TUS is enabled.
        services.AddSingleton<MediaTusMarker>();

        // Default temp store (local disk). Cloud modules replace this via ITusTempStore.
        services.TryAddSingleton<ITusTempStore, DiskTusTempStore>();

        // Distributed store and metadata — works in-memory by default, Redis when available.
        services.AddSingleton<DistributedMediaTusStore>();
        services.AddSingleton<DistributedTusUploadMetadataStore>();
        services.AddSingleton<DistributedFileLockProvider>();
    }

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider
    )
    {
        // Disable Kestrel/IIS request body size limit AND ASP.NET Core form size
        // limit for TUS uploads. This MUST run as early middleware — before any
        // other middleware reads the request body — otherwise the server rejects
        // the request with "Request body too large".
        // tusdotnet enforces its own size limit via MaxAllowedUploadSizeInBytesLong.
        app.Use(
            async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/media/tus"))
                {
                    // Disable Kestrel's MaxRequestBodySize.
                    var maxRequestBodySizeFeature =
                        context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                    if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
                    {
                        maxRequestBodySizeFeature.MaxRequestBodySize = null;
                    }

                    // Override the form feature so that if anything downstream tries to
                    // read the request as a form, the body length limit won't block it.
                    var formFeature = context.Features.Get<IFormFeature>();
                    if (formFeature == null || formFeature.Form == null)
                    {
                        context.Features.Set<IFormFeature>(
                            new FormFeature(
                                context.Request,
                                new FormOptions { MultipartBodyLengthLimit = long.MaxValue }
                            )
                        );
                    }
                }

                await next();
            }
        );

        routes.MapTus(
            "/api/media/tus",
            async httpContext =>
            {
                var authService =
                    httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                if (
                    !await authService.AuthorizeAsync(
                        httpContext.User,
                        MediaPermissions.ManageMedia
                    )
                )
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return null;
                }

                var store =
                    httpContext.RequestServices.GetRequiredService<DistributedMediaTusStore>();
                var mediaOptions = httpContext.RequestServices.GetRequiredService<
                    IOptions<MediaOptions>
                >();
                var fileLockProvider =
                    httpContext.RequestServices.GetRequiredService<DistributedFileLockProvider>();

                return new DefaultTusConfiguration
                {
                    Store = store,
                    FileLockProvider = fileLockProvider,
                    MaxAllowedUploadSizeInBytesLong = mediaOptions.Value.MaxFileSize,
                    Expiration = new tusdotnet.Models.Expiration.SlidingExpiration(
                        mediaOptions.Value.TemporaryFileLifetime
                    ),
                    Events = new tusdotnet.Models.Configuration.Events
                    {
                        OnBeforeCreateAsync = async ctx =>
                        {
                            // Validate file extension and destination path from metadata.
                            var metadata = ctx.Metadata;

                            if (!metadata.TryGetValue("fileName", out var fileNameMeta))
                            {
                                ctx.FailRequest("Missing required metadata: fileName");
                                return;
                            }

                            var fileName = fileNameMeta.GetString(System.Text.Encoding.UTF8);
                            var extension = Path.GetExtension(fileName);

                            if (
                                !mediaOptions.Value.AllowedFileExtensions.Contains(
                                    extension,
                                    StringComparer.OrdinalIgnoreCase
                                )
                            )
                            {
                                ctx.FailRequest($"File extension not allowed: {extension}");
                                return;
                            }

                            // Validate folder permissions if destinationPath is provided.
                            if (metadata.TryGetValue("destinationPath", out var destMeta))
                            {
                                var destinationPath = destMeta.GetString(System.Text.Encoding.UTF8);
                                if (
                                    !await authService.AuthorizeAsync(
                                        httpContext.User,
                                        MediaPermissions.ManageMediaFolder,
                                        (object)destinationPath
                                    )
                                )
                                {
                                    ctx.FailRequest(
                                        "You do not have permission to upload to this folder."
                                    );
                                    return;
                                }
                            }
                        },

                        OnCreateCompleteAsync = async ctx =>
                        {
                            // Store upload metadata for later retrieval.
                            var metadata = ctx.Metadata;

                            var fileName = metadata.TryGetValue("fileName", out var fileNameMeta)
                                ? fileNameMeta.GetString(System.Text.Encoding.UTF8)
                                : string.Empty;
                            var destinationPath = metadata.TryGetValue(
                                "destinationPath",
                                out var destMeta
                            )
                                ? destMeta.GetString(System.Text.Encoding.UTF8)
                                : string.Empty;

                            // Normalize file name if the service is available.
                            var nameNormalizer =
                                httpContext.RequestServices.GetService<IMediaNameNormalizerService>();
                            if (nameNormalizer != null)
                            {
                                fileName = nameNormalizer.NormalizeFileName(fileName);
                            }

                            var metadataStore =
                                httpContext.RequestServices.GetRequiredService<DistributedTusUploadMetadataStore>();
                            await metadataStore.SetAsync(
                                ctx.FileId,
                                new TusUploadEntry
                                {
                                    DestinationPath = destinationPath,
                                    FileName = fileName,
                                },
                                ctx.CancellationToken
                            );
                        },

                        OnFileCompleteAsync = async ctx =>
                        {
                            // Move completed file from temp to IMediaFileStore.
                            var metadataStore =
                                httpContext.RequestServices.GetRequiredService<DistributedTusUploadMetadataStore>();
                            var entry = await metadataStore.GetAsync(
                                ctx.FileId,
                                ctx.CancellationToken
                            );

                            if (entry == null)
                            {
                                return;
                            }

                            var mediaFileStore =
                                httpContext.RequestServices.GetRequiredService<IMediaFileStore>();
                            var mediaFilePath = mediaFileStore.Combine(
                                entry.DestinationPath,
                                entry.FileName
                            );

                            using var stream = store.OpenReadStream(ctx.FileId);
                            var finalPath = await mediaFileStore.CreateFileFromStreamAsync(
                                mediaFilePath,
                                stream
                            );
                            entry.MediaFilePath = finalPath;

                            // Persist the updated entry (with MediaFilePath) back to the distributed cache
                            // so GetTusFileInfo can retrieve it.
                            await metadataStore.SetAsync(ctx.FileId, entry, ctx.CancellationToken);

                            // Clean up the temp file.
                            await store.DeleteFileAsync(ctx.FileId, ctx.CancellationToken);
                        },
                    },
                };
            }
        );
    }
}

[Feature("OrchardCore.Media.SignalR")]
public sealed class MediaSignalRStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IMediaEventHandler, MediaSignalREventHandler>();
    }

    public override void Configure(
        IApplicationBuilder app,
        IEndpointRouteBuilder routes,
        IServiceProvider serviceProvider)
    {
        routes.MapHub<MediaHub>("/hubs/media");
    }
}

[Feature("OrchardCore.Media.SignalR.Azure")]
public sealed class MediaSignalRAzureStartup : StartupBase
{
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public MediaSignalRAzureStartup(
        IShellConfiguration configuration,
        ILogger<MediaSignalRAzureStartup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration
            .GetSection("OrchardCore_Media_SignalR")
            .GetValue<string>("ConnectionString");

        if (!string.IsNullOrEmpty(connectionString))
        {
            _logger.LogInformation("Azure SignalR Service is enabled for media real-time updates.");
            services.AddSignalR().AddAzureSignalR(connectionString);
        }
        else
        {
            _logger.LogWarning(
                "OrchardCore.Media.SignalR.Azure feature is enabled but 'OrchardCore_Media_SignalR:ConnectionString' is not configured.");
        }
    }
}

[Feature("OrchardCore.Media.SignalR.Redis")]
public sealed class MediaSignalRRedisStartup : StartupBase
{
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public MediaSignalRRedisStartup(
        IShellConfiguration configuration,
        ILogger<MediaSignalRRedisStartup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration
            .GetSection("OrchardCore_Redis")
            .GetValue<string>("Configuration");

        if (!string.IsNullOrEmpty(connectionString))
        {
            _logger.LogInformation("Redis backplane is enabled for media SignalR.");
            services.AddSignalR().AddStackExchangeRedis(connectionString);
        }
        else
        {
            _logger.LogWarning(
                "OrchardCore.Media.SignalR.Redis feature is enabled but 'OrchardCore_Redis:Configuration' is not configured.");
        }
    }
}
