using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Orchard.Localization.Core
{
    public class StringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly ILogger _logger;

        public StringLocalizerFactory(ILocalizationManager localizationManager, ILogger<StringLocalizerFactory> logger)
        {
            _localizationManager = localizationManager;
            _logger = logger;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return new StringLocalizer(CultureInfo.CurrentUICulture, resourceSource.FullName, _localizationManager, _logger);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var relativeName = baseName.Replace(location, string.Empty).Trim('.');
            return new StringLocalizer(CultureInfo.CurrentUICulture, relativeName, _localizationManager, _logger);
        }
    }
}
