using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Media.Services;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure
{
    [Feature("OrchardCore.Media.Azure.Storage")]
    public class Startup : StartupBase
    {
        /// <summary>
        /// The url prefix used to route asset files
        /// </summary>
        private const string AssetsUrlPrefix = "/media";

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

                    var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

                    // To make the 'BlobFileStore' tenant aware.
                    if (shellSettings.RequestUrlPrefix != null)
                    {
                        options.BasePath = '/' + shellSettings.RequestUrlPrefix;
                    }

                    var fileStore = new BlobFileStore(options, clock);

                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var pathBase = httpContextAccessor.HttpContext.Request.PathBase;

                    // 'PathBase' includes the 'RequestUrlPrefix' and may start by a virtual folder.
                    var mediaUrlBase = pathBase.Add(AssetsUrlPrefix);

                    return new MediaFileStore(fileStore, mediaUrlBase);
                }));
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaBlobStorageOptionsCheckFilter));
            });
        }
    }
}
