using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly bool _fallBackToParentCulture;
        private readonly ILogger _logger;

        public PortableObjectStringLocalizerFactory(
            ILocalizationManager localizationManager,
            IOptions<RequestLocalizationOptions> requestLocalizationOptions,
            ILogger<PortableObjectStringLocalizerFactory> logger)
        {
            _localizationManager = localizationManager;
            _fallBackToParentCulture = requestLocalizationOptions.Value.FallBackToParentUICultures;
            _logger = logger;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return new PortableObjectStringLocalizer(resourceSource.FullName, _localizationManager, _fallBackToParentCulture, _logger);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var index = 0;
            if (baseName.StartsWith(location, StringComparison.OrdinalIgnoreCase))
            {
                index = location.Length;
            }

            if (baseName.Length > index && baseName[index] == '.')
            {
                index += 1;
            }

            if (baseName.Length > index && baseName.IndexOf("Areas.", index) == index)
            {
                index += "Areas.".Length;
            }

            var relativeName = baseName.Substring(index);

            return new PortableObjectStringLocalizer(relativeName, _localizationManager, _fallBackToParentCulture, _logger);
        }
    }
}
