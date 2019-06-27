using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Media.AliYun
{
    public class MediaOssStorageOptionsCheckFilter : ActionFilterAttribute
    {
        private readonly MediaOssStorageOptions _options;
        private readonly ILogger<MediaOssStorageOptionsCheckFilter> _logger;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<MediaOssStorageOptionsCheckFilter> T;

        public MediaOssStorageOptionsCheckFilter(
            IOptions<MediaOssStorageOptions> options, 
            ILogger<MediaOssStorageOptionsCheckFilter> logger, 
            INotifier notifier, 
            IHtmlLocalizer<MediaOssStorageOptionsCheckFilter> localizer)
        {
            _options = options.Value;
            _logger = logger;
            _notifier = notifier;
            T = localizer;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (!CheckOptions(_options.Endpoint, _options.BucketName, _logger) && AdminAttribute.IsApplied(context.HttpContext))
                _notifier.Error(T["Azure Media Storage is enabled but not active because some settings are missing or invalid. Check the error log for details, then correct the settings and restart the application."]);

            base.OnActionExecuted(context);
        }

        public static bool CheckOptions(string endpoint, string bucketName, ILogger logger)
        {
            var optionsAreValid = true;

            if (String.IsNullOrWhiteSpace(endpoint))
            {
                logger.LogError($"Azure Media Storage is enabled but not active because {nameof(MediaOssStorageOptions.Endpoint)} is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            if (String.IsNullOrWhiteSpace(bucketName))
            {
                logger.LogError($"Azure Media Storage is enabled but not active because {nameof(MediaOssStorageOptions.Endpoint)} is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            return optionsAreValid;
        }
    }
}
