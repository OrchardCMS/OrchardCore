using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Media.Azure.Services;
using OrchardCore.Media.Core;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Events;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Caching.Azure;

namespace OrchardCore.Media.Azure;

[Feature("OrchardCore.Media.Azure.Storage")]
public sealed class Startup : Modules.StartupBase
{
    private readonly ILogger _logger;
    private readonly IShellConfiguration _configuration;

    public Startup(ILogger<Startup> logger, IShellConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override int Order
        => OrchardCoreConstants.ConfigureOrder.AzureMediaStorage;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddTransient<IConfigureOptions<MediaBlobStorageOptions>, MediaBlobStorageOptionsConfiguration>();

        // Only replace default implementation if options are valid.
        var section = _configuration.GetSection("OrchardCore_Media_Azure");
        var connectionString = section.GetValue<string>(nameof(MediaBlobStorageOptions.ConnectionString));
        var containerName = section.GetValue<string>(nameof(MediaBlobStorageOptions.ContainerName));

        if (CheckOptions(connectionString, containerName, _logger))
        {
            // Register a media cache file provider.
            services.AddSingleton<IMediaFileStoreCacheFileProvider>(serviceProvider =>
            {
                var hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

                if (string.IsNullOrWhiteSpace(hostingEnvironment.WebRootPath))
                {
                    throw new MediaConfigurationException("The wwwroot folder for serving cache media files is missing.");
                }

                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStoreCacheFileProvider>>();

                var mediaCachePath = GetMediaCachePath(
                    hostingEnvironment, shellSettings, DefaultMediaFileStoreCacheFileProvider.AssetsCachePath);

                if (!Directory.Exists(mediaCachePath))
                {
                    Directory.CreateDirectory(mediaCachePath);
                }

                return new DefaultMediaFileStoreCacheFileProvider(logger, mediaOptions.AssetsRequestPath, mediaCachePath);
            });

            // Replace the default media file provider with the media cache file provider.
            services.Replace(ServiceDescriptor.Singleton<IMediaFileProvider>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>()));

            // Register the media cache file provider as a file store cache provider.
            services.AddSingleton<IMediaFileStoreCache>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>());

            // Replace the default media file store with a blob file store.
            services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
            {
                var blobStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
                var shellOptions = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                var clock = serviceProvider.GetRequiredService<IClock>();
                var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();
                var mediaEventHandlers = serviceProvider.GetServices<IMediaEventHandler>();
                var mediaCreatingEventHandlers = serviceProvider.GetServices<IMediaCreatingEventHandler>();
                var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStore>>();

                var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);
                var mediaUrlBase = "/" + fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath);

                var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext
                    ?.Features.Get<ShellContextFeature>()
                    ?.OriginalPathBase ?? PathString.Empty;

                if (originalPathBase.HasValue)
                {
                    mediaUrlBase = fileStore.Combine(originalPathBase.Value, mediaUrlBase);
                }

                return new DefaultMediaFileStore(fileStore, mediaUrlBase, mediaOptions.CdnBaseUrl, mediaEventHandlers, mediaCreatingEventHandlers, logger);
            }));

            services.AddSingleton<IMediaEventHandler, DefaultMediaFileStoreCacheEventHandler>();

            services.AddScoped<IModularTenantEvents, MediaBlobContainerTenantEvents>();
        }
    }

    private static string GetMediaCachePath(IWebHostEnvironment hostingEnvironment, ShellSettings shellSettings, string assetsPath)
        => PathExtensions.Combine(hostingEnvironment.WebRootPath, shellSettings.Name, assetsPath);

    private static bool CheckOptions(string connectionString, string containerName, ILogger logger)
    {
        var optionsAreValid = true;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogError("Azure Media Storage is enabled but not active because the 'ConnectionString' is missing or empty in application configuration.");
            optionsAreValid = false;
        }

        if (string.IsNullOrWhiteSpace(containerName))
        {
            logger.LogError("Azure Media Storage is enabled but not active because the 'ContainerName' is missing or empty in application configuration.");
            optionsAreValid = false;
        }

        return optionsAreValid;
    }
}

[Feature("OrchardCore.Media.Azure.ImageSharpImageCache")]
public sealed class ImageSharpAzureBlobCacheStartup : Modules.StartupBase
{
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public ImageSharpAzureBlobCacheStartup(
        IShellConfiguration configuration,
        ILogger<ImageSharpAzureBlobCacheStartup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override int Order
        => OrchardCoreConstants.ConfigureOrder.AzureImageSharpCache;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ImageSharpBlobImageCacheOptions>, ImageSharpBlobImageCacheOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<AzureBlobStorageCacheOptions>, AzureBlobStorageCacheOptionsConfiguration>();

        // Only replace default implementation if options are valid.
        var section = _configuration.GetSection("OrchardCore_Media_Azure_ImageSharp_Cache");
        var connectionString = section.GetValue<string>(nameof(MediaBlobStorageOptions.ConnectionString));
        var containerName = section.GetValue<string>(nameof(MediaBlobStorageOptions.ContainerName));

        if (!CheckOptions(connectionString, containerName))
        {
            return;
        }

        // Following https://docs.sixlabors.com/articles/imagesharp.web/imagecaches.html we'd use
        // SetCache<AzureBlobStorageCache>() but that's only available on IImageSharpBuilder after AddImageSharp(),
        // what happens in OrchardCore.Media. Thus, an explicit Replace() is necessary.
        services.Replace(ServiceDescriptor.Singleton<IImageCache, AzureBlobStorageCache>());

        services.AddScoped<IModularTenantEvents, ImageSharpBlobImageCacheTenantEvents>();
    }

    private bool CheckOptions(string connectionString, string containerName)
    {
        var optionsAreValid = true;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError(
                "Azure Media ImageSharp Image Cache is enabled but not active because the 'ConnectionString' is missing or empty in application configuration.");
            optionsAreValid = false;
        }

        if (string.IsNullOrWhiteSpace(containerName))
        {
            _logger.LogError(
                "Azure Media ImageSharp Image Cache is enabled but not active because the 'ContainerName' is missing or empty in application configuration.");
            optionsAreValid = false;
        }

        return optionsAreValid;
    }
}
