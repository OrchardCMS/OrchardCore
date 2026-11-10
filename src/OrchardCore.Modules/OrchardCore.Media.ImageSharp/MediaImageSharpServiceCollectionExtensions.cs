using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.AWS.Caching;
using SixLabors.ImageSharp.Web.Azure.Caching;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;

namespace OrchardCore.Media.ImageSharp;

/// <summary>
/// Adds the ImageSharp based media image processing. This assembly is the only one referencing
/// the Six Labors packages, see the comment of its project file.
/// </summary>
public static class MediaImageSharpServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services resizing the media images, and cleaning their local cache.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="cacheFolder">The folder of the local cache of the resized images, relative to the web root.</param>
    public static IServiceCollection AddMediaImageSharp(this IServiceCollection services, string cacheFolder)
    {
        services.AddSingleton<IBackgroundTask, ResizedMediaCacheBackgroundTask>();

        // Add ImageSharp Configuration first, to override ImageSharp defaults.
        services.AddTransient<IConfigureOptions<ImageSharpMiddlewareOptions>, MediaImageSharpConfiguration>();

        services
            .AddImageSharp()
            .RemoveProvider<PhysicalFileSystemProvider>()
            // For multitenancy we must use an absolute path to prevent leakage across tenants on different hosts.
            .SetCacheKey<BackwardsCompatibleCacheKey>()
            .Configure<PhysicalFileSystemCacheOptions>(options =>
            {
                options.CacheFolder = cacheFolder;
                options.CacheFolderDepth = 12;
            })
            .AddProvider<MediaResizingFileProvider>()
            .AddProcessor<ImageVersionProcessor>()
            .AddProcessor<TokenCommandProcessor>();

        services.AddSingleton<IMediaTokenService, MediaTokenService>();

        return services;
    }

    /// <summary>
    /// Stores the resized images in Azure Blob Storage, as configured by the
    /// <see cref="MediaImageSharpAzureBlobCacheOptions"/>, instead of the local cache.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddMediaImageSharpAzureBlobCache(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureBlobStorageCacheOptions>, AzureBlobStorageCacheOptionsConfiguration>();

        // Following https://docs.sixlabors.com/articles/imagesharp.web/imagecaches.html we'd use
        // SetCache<AzureBlobStorageCache>() but that's only available on IImageSharpBuilder after AddImageSharp(),
        // what happens in OrchardCore.Media. Thus, an explicit Replace() is necessary.
        services.Replace(ServiceDescriptor.Singleton<IImageCache, AzureBlobStorageCache>());

        return services;
    }

    /// <summary>
    /// Stores the resized images in Amazon S3, as configured by the
    /// <see cref="MediaImageSharpAmazonS3CacheOptions"/>, instead of the local cache.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddMediaImageSharpAmazonS3Cache(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AWSS3StorageCacheOptions>, AWSS3StorageCacheOptionsConfiguration>();

        // Following https://docs.sixlabors.com/articles/imagesharp.web/imagecaches.html we'd use
        // SetCache<AWSS3StorageCache>() but that's only available on IImageSharpBuilder after AddImageSharp(),
        // what happens in OrchardCore.Media. Thus, an explicit Replace() is necessary.
        services.Replace(ServiceDescriptor.Singleton<IImageCache, AWSS3StorageCache>());

        return services;
    }

    private sealed class AzureBlobStorageCacheOptionsConfiguration : IConfigureOptions<AzureBlobStorageCacheOptions>
    {
        private readonly MediaImageSharpAzureBlobCacheOptions _options;

        public AzureBlobStorageCacheOptionsConfiguration(IOptions<MediaImageSharpAzureBlobCacheOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(AzureBlobStorageCacheOptions options)
        {
            options.ConnectionString = _options.ConnectionString;
            options.ContainerName = _options.ContainerName;
            options.CacheFolder = _options.CacheFolder;
        }
    }

    private sealed class AWSS3StorageCacheOptionsConfiguration : IConfigureOptions<AWSS3StorageCacheOptions>
    {
        private readonly MediaImageSharpAmazonS3CacheOptions _options;

        public AWSS3StorageCacheOptionsConfiguration(IOptions<MediaImageSharpAmazonS3CacheOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(AWSS3StorageCacheOptions options)
        {
            options.Endpoint = _options.Endpoint;
            options.Region = _options.Region;
            options.BucketName = _options.BucketName;
            options.AccessKey = _options.AccessKey;
            options.AccessSecret = _options.AccessSecret;
            options.CacheFolder = _options.CacheFolder;
        }
    }
}
