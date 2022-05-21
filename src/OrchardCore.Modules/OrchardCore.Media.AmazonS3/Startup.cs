using System;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Modules;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media.Core.Events;

namespace OrchardCore.Media.AmazonS3;

[Feature("OrchardCore.Media.AmazonS3.Storage")]
public class Startup : Modules.StartupBase
{
    private readonly ILogger _logger;
    private readonly IShellConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public Startup(IShellConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        ILogger<Startup> logger)
        => (_configuration, _webHostEnvironment, _logger)
            = (configuration, webHostEnvironment, logger);

    public override void ConfigureServices(IServiceCollection services)
    {
        var storeOptions = new AwsStorageOptions().BindConfiguration(_configuration);

        var validationErrors = storeOptions.Validate().ToList();
        var stringBuilder = new StringBuilder();

        if (validationErrors.Any())
        {
            foreach (var error in validationErrors)
            {
                stringBuilder.Append(error.ErrorMessage);
            }

            if (_webHostEnvironment.IsDevelopment())
            {
                _logger.LogInformation(
                    $"S3 Media configuration validation failed: {validationErrors} fallback to File storage", stringBuilder);
            }
            else
            {
                _logger.LogError(
                    $"S3 Media configuration validation failed with errors: {validationErrors} fallback to File storage", stringBuilder);
            }
        }
        else
        {
            _logger.LogInformation(
                $"Starting with S3 Media Configuration. {System.Environment.NewLine} BucketName: {storeOptions.BucketName}, {System.Environment.NewLine} BasePath: {storeOptions.BasePath}");

            services.AddSingleton<IMediaFileStoreCacheFileProvider>(serviceProvider =>
            {
                var hostingEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();

                if (String.IsNullOrWhiteSpace(hostingEnvironment.WebRootPath))
                {
                    throw new Exception("The wwwroot folder for serving cache media files is missing.");
                }

                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStoreCacheFileProvider>>();

                var mediaCachePath = GetMediaCachePath(hostingEnvironment,
                    DefaultMediaFileStoreCacheFileProvider.AssetsCachePath, shellSettings);

                if (!Directory.Exists(mediaCachePath))
                {
                    Directory.CreateDirectory(mediaCachePath);
                }

                return new DefaultMediaFileStoreCacheFileProvider(logger, mediaOptions.AssetsRequestPath,
                    mediaCachePath);
            });

            // Replace the default media file provider with the media cache file provider.
            services.Replace(ServiceDescriptor.Singleton<IMediaFileProvider>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>()));

            // Register the media cache file provider as a file store cache provider.
            services.AddSingleton<IMediaFileStoreCache>(serviceProvider =>
                serviceProvider.GetRequiredService<IMediaFileStoreCacheFileProvider>());

            services.AddSingleton<IAmazonS3>(serviceProvider =>
            {
                var options = storeOptions;
                if (options.Credentials == null)
                {
                    return new AmazonS3Client();
                }

                var config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(options.Credentials.RegionEndpoint),
                    UseHttp = true,
                    ForcePathStyle = true,
                    UseArnRegion = true
                };

                if (String.IsNullOrWhiteSpace(options.Credentials.AccessKeyId))
                {
                    return new AmazonS3Client(new ECSTaskCredentials(), config);
                }

                return new AmazonS3Client(options.Credentials.AccessKeyId,
                    options.Credentials.SecretKey,
                    config);
            });

            services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
            {
                var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
                var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                var mediaEventHandlers = serviceProvider.GetServices<IMediaEventHandler>();
                var mediaCreatingEventHandlers = serviceProvider.GetServices<IMediaCreatingEventHandler>();
                var clock = serviceProvider.GetRequiredService<IClock>();
                var logger = serviceProvider.GetRequiredService<ILogger<DefaultMediaFileStore>>();
                var amazonS3Client = serviceProvider.GetService<IAmazonS3>();

                var fileStore = new AwsFileStore(clock, storeOptions, amazonS3Client);

                var mediaUrlBase =
                    $"/{fileStore.Combine(shellSettings.RequestUrlPrefix, mediaOptions.AssetsRequestPath)}";

                var originalPathBase = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext
                    ?.Features.Get<ShellContextFeature>()
                    ?.OriginalPathBase;

                if (originalPathBase.HasValue && !String.IsNullOrWhiteSpace(originalPathBase.Value))
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
        }
    }

    private string GetMediaCachePath(IWebHostEnvironment hostingEnvironment,
        string assetsPath, ShellSettings shellSettings)
        => PathExtensions.Combine(hostingEnvironment.WebRootPath,
            assetsPath, shellSettings.Name);
}
