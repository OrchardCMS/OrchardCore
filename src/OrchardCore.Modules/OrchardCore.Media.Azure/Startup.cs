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
using SixLabors.ImageSharp.Web.Providers;
using OrchardCore.Media.Azure.Processing;

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
            if (mediaBlobStorageOptions == null)
            {
                mediaBlobStorageOptions = new MediaBlobStorageOptions();
            }
            services.Configure<MediaBlobStorageOptions>(mediaBlobConfiguration);
            //TODO also remove IMediaFileProvider and IStaticFileProvider

            // Only replace default implementation if options are valid.
            var mediaBlobCheck = MediaBlobStorageOptionsCheckFilter.CheckOptions(mediaBlobStorageOptions.ConnectionString, mediaBlobStorageOptions.ContainerName, _logger);
            if (mediaBlobCheck)
            {
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var mediaOptions = serviceProvider.GetRequiredService<IOptions<MediaOptions>>().Value;
                    var blobStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
                    var clock = serviceProvider.GetRequiredService<IClock>();
                    var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();
                    var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);

                    // This doesn't work here, container is built (so readonly)
                    if (!blobStorageOptions.SupportResizing)
                    {
                        services.Replace(ServiceDescriptor.Singleton<IMediaFileStorePathProvider>(sp =>
                        {
                            var mediaBaseUri = fileStore.BaseUri;
                            if (!String.IsNullOrEmpty(blobStorageOptions.PublicHostName))
                                mediaBaseUri = new UriBuilder(mediaBaseUri) { Host = blobStorageOptions.PublicHostName }.Uri;

                            return new MediaBlobFileStorePathProvider(mediaBaseUri.ToString());
                        }));
                    } else
                    {
                        services.Replace(ServiceDescriptor.Singleton<IImageProvider, MediaBlobResizingFileProvider>());
                    }
                    var mediaFileStorePathProvider = serviceProvider.GetRequiredService<IMediaFileStorePathProvider>();
                    return new MediaFileStore(fileStore, mediaFileStorePathProvider);
                }));

                if (mediaBlobStorageOptions.SupportResizing)
                {
                    services.Replace(ServiceDescriptor.Singleton<IImageProvider, MediaBlobResizingFileProvider>());
                } else
                {
                    // do this
                    //services.Replace(ServiceDescriptor.Singleton<IMediaFileStorePathProvider>(sp =>
                    //{
                    //    var mediaBaseUri = fileStore.BaseUri;
                    //    if (!String.IsNullOrEmpty(blobStorageOptions.PublicHostName))
                    //        mediaBaseUri = new UriBuilder(mediaBaseUri) { Host = blobStorageOptions.PublicHostName }.Uri;

                    //    return new MediaBlobFileStorePathProvider(mediaBaseUri.ToString());
                    //}));
                }
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
