using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Media.Azure.Middleware;
using OrchardCore.Media.Azure.Processing;
using OrchardCore.Media.Azure.Services;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using SixLabors.ImageSharp.Web.DependencyInjection;

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
                // Remove this specific IStaticFileProvider as we no longer need to provide this particular provider to the ShellFileVersionProvider.
                var staticFileProviderDescriptor = services.FirstOrDefault(descriptor =>
                    descriptor.ServiceType == typeof(IStaticFileProvider) &&
                    descriptor.ImplementationFactory.Method.ReturnType == typeof(IMediaFileProvider));

                if (staticFileProviderDescriptor != null)
                {
                    services.Remove(staticFileProviderDescriptor);
                }

                // Remove the IMediaFileProvider as we no longer need to serve from media from the StaticFileMiddleware.
                services.RemoveAll<IMediaFileProvider>();

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

                // Replace MediaFileResolverFactory with blob implementation.
                services.Replace(ServiceDescriptor.Singleton<IMediaFileResolverFactory, MediaBlobFileResolverFactory>());

                // Add Azure Blob FileStoreVersionProvider.
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IFileStoreVersionProvider, MediaBlobFileStoreVersionProvider>());
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaBlobStorageOptionsCheckFilter));
            });
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Only use middleware if options are valid, and services replaced.
            var mediaBlobStorageOptions = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
            if (MediaBlobStorageOptionsCheckFilter.CheckOptions(mediaBlobStorageOptions.ConnectionString, mediaBlobStorageOptions.ContainerName, _logger))
            {

                // Media filestore middleware before ImageSharp.
                app.UseMiddleware<MediaFileStoreMiddleware>();

                // ImageSharp after the media filestore middleware.
                app.UseImageSharp();

            }
        }
    }
}
