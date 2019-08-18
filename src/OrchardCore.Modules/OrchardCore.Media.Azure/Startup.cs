using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Media.Azure.Services;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;

namespace OrchardCore.Media.Azure
{
    [Feature("OrchardCore.Media.Azure.Storage")]
    public class Startup : StartupBase
    {
        private readonly ILogger<Startup> _logger;
        private readonly IShellConfiguration _configuration;

        public Startup(ILogger<Startup> logger, IShellConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MediaBlobStorageOptions>(_configuration.GetSection("OrchardCore.Media.Azure"));

            // Only replace default implementation if options are valid.
            var connectionString = _configuration[$"OrchardCore.Media.Azure:{nameof(MediaBlobStorageOptions.ConnectionString)}"];
            var containerName = _configuration[$"OrchardCore.Media.Azure:{nameof(MediaBlobStorageOptions.ContainerName)}"];
            if (MediaBlobStorageOptionsCheckFilter.CheckOptions(connectionString, containerName, _logger))
            {
                // Remove this specific IStaticFileProvider as we do not need to provide this to the ShellFileVersionProvider.
                var staticFileProviderDescriptor = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IStaticFileProvider) &&
                    descriptor.ImplementationFactory.Method.ReturnType == typeof(IMediaFileProvider));

                if (staticFileProviderDescriptor != null)
                {
                    services.Remove(staticFileProviderDescriptor);
                }

                // Replace the default media file provider with the media cache file provider.
                services.Replace(ServiceDescriptor.Singleton<IMediaFileProvider>(serviceProvider =>
                    serviceProvider.GetRequiredService<IMediaCacheFileProvider>()));

                // Register the media cache file provider as a file store cache provider.
                services.AddSingleton<IMediaFileStoreCacheProvider>(serviceProvider =>
                    (IMediaFileStoreCacheProvider)serviceProvider.GetRequiredService<IMediaCacheFileProvider>());

                // Replace the default media file store with a blob file store.
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var blobStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
                    var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>();
                    var clock = serviceProvider.GetRequiredService<IClock>();
                    var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();
                    var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);
                    var mediaFileStorePathProvider = serviceProvider.GetRequiredService<IMediaFileStorePathProvider>();
                    return new MediaFileStore(fileStore, mediaFileStorePathProvider, mediaOptions);
                }));

                // Add blob file store version provider.
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileStoreVersionProvider, MediaBlobFileStoreVersionProvider>());
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaBlobStorageOptionsCheckFilter));
            });
        }
    }
}
