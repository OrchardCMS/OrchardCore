using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AliYunOss;
using OrchardCore.Media.Services;
using OrchardCore.Modules;

namespace OrchardCore.Media.AliYun
{
    [Feature("OrchardCore.Media.AliYun.Storage")]
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

            services.Configure<MediaOssStorageOptions>(_configuration.GetSection("OrchardCore.Media.AliYun"));

            // Only replace default implementation if options are valid.
            var connectionString = _configuration[$"OrchardCore.Media.AliYun:{nameof(MediaOssStorageOptions.Endpoint)}"];
            var containerName = _configuration[$"OrchardCore.Media.AliYun:{nameof(MediaOssStorageOptions.Endpoint)}"];
            if (MediaOssStorageOptionsCheckFilter.CheckOptions(connectionString, containerName, _logger))
            {
                services.Replace(ServiceDescriptor.Singleton<IMediaFileStore>(serviceProvider =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<MediaOssStorageOptions>>().Value;
                    var clock = serviceProvider.GetRequiredService<IClock>();

                    var fileStore = new OssFileStore(options, clock);

                    var mediaBaseUri = fileStore.BaseUri;
                    if (!String.IsNullOrEmpty(options.PublicHostName))
                        mediaBaseUri = new UriBuilder(mediaBaseUri) { Host = options.PublicHostName }.Uri;

                    return new MediaFileStore(fileStore, mediaBaseUri.ToString());
                }));
            }

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(MediaOssStorageOptionsCheckFilter));
            });
        }
    }
}
