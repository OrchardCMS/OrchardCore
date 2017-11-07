using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptionsCheckFilter : ActionFilterAttribute
    {
        private readonly MediaBlobStorageOptions _options;
        private readonly ILogger<MediaBlobStorageOptionsCheckFilter> _logger;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<MediaBlobStorageOptionsCheckFilter> T;

        public MediaBlobStorageOptionsCheckFilter(
            IOptions<MediaBlobStorageOptions> options, 
            ILogger<MediaBlobStorageOptionsCheckFilter> logger, 
            INotifier notifier, 
            IHtmlLocalizer<MediaBlobStorageOptionsCheckFilter> localizer)
        {
            _options = options.Value;
            _logger = logger;
            _notifier = notifier;
            T = localizer;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (!CheckOptions(_options.ConnectionString, _options.ContainerName, _logger) && AdminAttribute.IsApplied(context.HttpContext))
                _notifier.Error(T["Azure Media Storage is enabled but not active because some settings are missing or invalid. Check the error log for details, then correct the settings and restart the application."]);

            base.OnActionExecuted(context);
        }

        public static bool CheckOptions(string connectionString, string containerName, ILogger logger)
        {
            var optionsAreValid = true;

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogError($"Azure Media Storage is enabled but not active because {nameof(MediaBlobStorageOptions.ConnectionString)} is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            if (String.IsNullOrWhiteSpace(containerName))
            {
                logger.LogError($"Azure Media Storage is enabled but not active because {nameof(MediaBlobStorageOptions.ContainerName)} is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            return optionsAreValid;
        }
    }
}
