using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Media.Services;
using OrchardCore.Modules;
using Microsoft.Extensions.Configuration;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Azure
{
    [Feature("OrchardCore.Media.Azure.Storage")]
    public class Startup : StartupBase
    {
        private ILogger<Startup> _logger;
        private readonly IShellConfiguration _configuration;

        public Startup(ILogger<Startup> logger, IShellConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            var mediaBlobConfiguration = _configuration.GetSection("OrchardCore.Media.Azure");
            var mediaBlobStorageOptions = mediaBlobConfiguration.Get<MediaBlobStorageOptions>();
            services.Configure<MediaBlobStorageOptions>(mediaBlobConfiguration);

            // Only replace default implementation if options are valid.
            if (MediaBlobStorageOptionsCheckFilter.CheckOptions(mediaBlobStorageOptions.ConnectionString, mediaBlobStorageOptions.ContainerName, _logger))
            {
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                    var blobStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
                    var clock = serviceProvider.GetRequiredService<IClock>();
                    var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();
                    var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);

                    if (!blobStorageOptions.SupportResizing)
                    {
                        services.Replace(ServiceDescriptor.Singleton<IMediaFileStorePathProvider>(sp =>
                        {
                            var mediaBaseUri = fileStore.BaseUri;
                            if (!String.IsNullOrEmpty(blobStorageOptions.PublicHostName))
                                mediaBaseUri = new UriBuilder(mediaBaseUri) { Host = blobStorageOptions.PublicHostName }.Uri;

                            return new MediaBlobFileStorePathProvider(mediaBaseUri.ToString());
                        }));
                    }
                    var mediaFileStorePathProvider = serviceProvider.GetRequiredService<IMediaFileStorePathProvider>();
                    return new MediaFileStore(fileStore, mediaFileStorePathProvider);
                }));

                services.AddSingleton<IFileStore>(serviceProvider => serviceProvider.GetRequiredService<IMediaFileStore>());

                services.AddSingleton<ICdnPathProvider>(serviceProvider => serviceProvider.GetRequiredService<IMediaFileStore>());
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaBlobStorageOptionsCheckFilter));
            });
        }
    }
}
