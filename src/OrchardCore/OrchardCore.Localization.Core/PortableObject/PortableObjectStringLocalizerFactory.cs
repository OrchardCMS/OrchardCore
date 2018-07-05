using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Localization.Services;
using OrchardCore.Modules.Services;

namespace OrchardCore.Localization.PortableObject
{
    public class PortableObjectStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public PortableObjectStringLocalizerFactory(
            ILocalizationManager localizationManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PortableObjectStringLocalizerFactory> logger)
        {
            _localizationManager = localizationManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            var culture = _httpContextAccessor.HttpContext.RequestServices
                .GetRequiredService<ILocalCulture>().GetLocalCultureAsync().Result;

            return new PortableObjectStringLocalizer(culture, resourceSource.FullName, _localizationManager, _logger);
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

            if (baseName.Length > index && baseName.IndexOf(".Modules.", index) == index)
            {
                index += ".Modules.".Length;
            }

            var relativeName = baseName.Substring(index);

            return new PortableObjectStringLocalizer(CultureInfo.CurrentUICulture, relativeName, _localizationManager, _logger);
        }
    }
}
