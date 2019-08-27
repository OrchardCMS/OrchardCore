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
            services.Configure<MediaBlobStorageOptions>(_configuration.GetSection("OrchardCore.Media.Azure"));

            // Only replace default implementation if options are valid.
            var connectionString = _configuration[$"OrchardCore.Media.Azure:{nameof(MediaBlobStorageOptions.ConnectionString)}"];
            var containerName = _configuration[$"OrchardCore.Media.Azure:{nameof(MediaBlobStorageOptions.ContainerName)}"];
            if (MediaBlobStorageOptionsCheckFilter.CheckOptions(connectionString, containerName, _logger))
            {
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
                    var clock = serviceProvider.GetRequiredService<IClock>();
                    var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();

                    var fileStore = new BlobFileStore(options, clock, contentTypeProvider);

                    var mediaBaseUri = fileStore.BaseUri;
                    if (!String.IsNullOrEmpty(options.PublicHostName))
                        mediaBaseUri = new UriBuilder(mediaBaseUri) { Host = options.PublicHostName }.Uri;

                    return new MediaFileStore(fileStore, mediaBaseUri.ToString());
                }));
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaBlobStorageOptionsCheckFilter));
            });
        }
    }
}
