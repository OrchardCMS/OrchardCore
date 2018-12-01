using System;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly ILogger _logger;

        public PortableObjectStringLocalizerFactory(
            ILocalizationManager localizationManager,
            ILogger<PortableObjectStringLocalizerFactory> logger)
        {
            _localizationManager = localizationManager;
            _logger = logger;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return new PortableObjectStringLocalizer(CultureInfo.CurrentUICulture, resourceSource.FullName, _localizationManager, _logger);
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

            return new PortableObjectStringLocalizer(CultureInfo.CurrentUICulture, relativeName, _localizationManager, _logger);
        }
    }
}
