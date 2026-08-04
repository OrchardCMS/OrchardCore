using System.Text;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media.AmazonS3.Services;
using OrchardCore.Media.Core;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Events;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Media.AmazonS3;

public sealed class Startup : Modules.StartupBase
{
    private readonly ILogger _logger;
    private readonly IShellConfiguration _configuration;

    public Startup(IShellConfiguration configuration,
        ILogger<Startup> logger)
        => (_configuration, _logger)
            = (configuration, logger);

    static Startup() => AWSConfigs.InitializeCollections = true;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddTransient<IConfigureOptions<AwsStorageOptions>, AwsStorageOptionsConfiguration>();

        var storeOptions = new AwsStorageOptions().BindConfiguration(AmazonS3Constants.ConfigSections.AmazonS3, _configuration, _logger);
        var validationErrors = storeOptions.Validate().ToList();
        var stringBuilder = new StringBuilder();

        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                stringBuilder.Append(error.ErrorMessage);
            }

            _logger.LogError("S3 Media configuration validation failed with errors: {Errors} fallback to local file storage.", stringBuilder);
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Starting with S3 Media Configuration. BucketName: {BucketName}; BasePath: {BasePath}", storeOptions.BucketName, storeOptions.BasePath);
            }

            services.AddSingleton<IMediaFileStoreCacheFileProvider>(serviceProvider =>
            {
                var hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

                if (string.IsNullOrWhiteSpace(hostingEnvironment.WebRootPath))
                {
                    throw new MediaConfigurationException("The wwwroot folder for serving cache media files is missing.");
                }

                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
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

            // Registering IAmazonS3 client using AWS registration factory.
            services.AddAWSService<IAmazonS3>(storeOptions.AwsOptions);

            services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
            {
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                var mediaEventHandlers = serviceProvider.GetServices<IMediaEventHandler>();
                var mediaCreatingEventHandlers = serviceProvider.GetServices<IMediaCreatingEventHandler>();
                var clock = serviceProvider.GetRequiredService<IClock>();
                var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStore>>();
                var amazonS3Client = serviceProvider.GetService<IAmazonS3>();

                var options = serviceProvider.GetRequiredService<IOptions<AwsStorageOptions>>();
                var fileStore = new AwsFileStore(clock, options.Value, amazonS3Client);

                var mediaUrlBase = $"/{fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath)}";

                var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext
                    ?.Features.Get<ShellContextFeature>()
                    ?.OriginalPathBase ?? PathString.Empty;

                if (originalPathBase.HasValue)
                {
                    mediaUrlBase = fileStore.Combine(originalPathBase.Value, mediaUrlBase);
                }

                return new DefaultMediaFileStore(fileStore,
                    mediaUrlBase,
                    mediaOptions.CdnBaseUrl,
                    mediaEventHandlers,
                    mediaCreatingEventHandlers,
                    logger);
            }));

            services.AddSingleton<IMediaEventHandler, DefaultMediaFileStoreCacheEventHandler>();

            services.AddScoped<IModularTenantEvents, MediaS3BucketTenantEvents>();
        }
    }

    private static string GetMediaCachePath(IWebHostEnvironment hostingEnvironment, ShellSettings shellSettings, string assetsPath)
        => PathExtensions.Combine(hostingEnvironment.WebRootPath, shellSettings.Name, assetsPath);
}

[Feature("OrchardCore.Media.AmazonS3.ImageCache")]
public sealed class MediaAmazonS3ImageCacheStartup : Modules.StartupBase
{
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public MediaAmazonS3ImageCacheStartup(
        IShellConfiguration configuration,
        ILogger<MediaAmazonS3ImageCacheStartup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AwsMediaImageCacheOptions>, AwsMediaImageCacheOptionsConfiguration>();

        var storeOptions = new AwsStorageOptions().BindConfiguration(AmazonS3Constants.ConfigSections.AmazonS3ImageCache, _configuration, _logger);
        var validationErrors = storeOptions.Validate().ToList();
        var stringBuilder = new StringBuilder();

        if (validationErrors.Count > 0)
        {
            foreach (var error in validationErrors)
            {
                stringBuilder.Append(error.ErrorMessage);
            }

            _logger.LogError("S3 Media Image Cache configuration validation failed with errors: {Errors} — falling back to local file storage.", stringBuilder);
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Starting with S3 Media Image Cache configuration. BucketName: {BucketName}; BasePath: {BasePath}", storeOptions.BucketName, storeOptions.BasePath);
            }

            services.Replace(ServiceDescriptor.Singleton<IResizedImageCache, AWSS3ResizedImageCache>());

            services.AddScoped<IModularTenantEvents, AwsS3MediaImageCacheTenantEvents>();
        }
    }
}

// Keeps the renamed feature enabled on sites that had the legacy
// "OrchardCore.Media.AmazonS3.ImageSharpImageCache" feature enabled before the rename. The legacy
// feature depends on the new one, and this migration explicitly enables the new feature so it
// remains active in its own right once the obsolete feature is removed.
[Feature("OrchardCore.Media.AmazonS3.ImageSharpImageCache")]
public sealed class LegacyImageCacheFeatureStartup : Modules.StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<LegacyImageCacheFeatureMigrations>();
    }
}

internal sealed class LegacyImageCacheFeatureMigrations : DataMigration
{
    public static int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            if (await featuresManager.IsFeatureEnabledAsync("OrchardCore.Media.AmazonS3.ImageCache"))
            {
                return;
            }

            await featuresManager.EnableFeaturesAsync("OrchardCore.Media.AmazonS3.ImageCache");
        });

        return 1;
    }
}
