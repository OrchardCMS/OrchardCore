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
using Microsoft.WindowsAzure.Storage;
using System.Linq;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Media.Azure.Services;

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

            // Only replace default implementation if options are valid.
            if (MediaBlobStorageOptionsCheckFilter.CheckOptions(mediaBlobStorageOptions.ConnectionString, mediaBlobStorageOptions.ContainerName, _logger))
            {
                // Remove the IMediaFileProvider & IStaticFileProvider, as we no longer need to serve from media from the file system
                services.RemoveAll<IMediaFileProvider>();
                var staticFileProviderDescriptor = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IStaticFileProvider) &&
                    descriptor.ImplementationFactory.Method.ReturnType == typeof(IMediaFileProvider));
                var staticFileProviderDescriptors = services.Where(x => x.ServiceType == typeof(IStaticFileProvider));
                if (staticFileProviderDescriptor != null)
                {
                    services.Remove(staticFileProviderDescriptor);
                }
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var blobStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
                    var clock = serviceProvider.GetRequiredService<IClock>();
                    var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();
                    var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);
                    var mediaFileStorePathProvider = serviceProvider.GetRequiredService<IMediaFileStorePathProvider>();
                    return new MediaFileStore(fileStore, mediaFileStorePathProvider);
                }));

                if (mediaBlobStorageOptions.SupportResizing)
                {
                    services.Replace(ServiceDescriptor.Singleton<IImageProvider, MediaBlobResizingFileProvider>());
                } else
                {
                    services.Replace(ServiceDescriptor.Singleton<IMediaFileStorePathProvider, MediaBlobFileStorePathProvider>());
                }

                services.AddSingleton<IFileStoreVersionProvider, MediaBlobFileStoreVersionProvider>();
                services.AddSingleton<ICdnPathProvider>(serviceProvider => serviceProvider.GetRequiredService<IMediaFileStore>());
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaBlobStorageOptionsCheckFilter));
            });
        }
    }
}
