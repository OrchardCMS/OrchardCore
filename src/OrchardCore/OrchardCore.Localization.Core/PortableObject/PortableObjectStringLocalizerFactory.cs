using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.PortableObject
{
    /// <summary>
    /// Represents a <see cref="IStringLocalizerFactory"/> for portable objects.
    /// </summary>
    public class PortableObjectStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ILocalizationManager _localizationManager;
        private readonly bool _fallBackToParentCulture;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of <see cref="PortableObjectStringLocalizerFactory"/>.
        /// </summary>
        /// <param name="localizationManager"></param>
        /// <param name="requestLocalizationOptions"></param>
        /// <param name="logger"></param>
        public PortableObjectStringLocalizerFactory(
            ILocalizationManager localizationManager,
            IOptions<RequestLocalizationOptions> requestLocalizationOptions,
            ILogger<PortableObjectStringLocalizerFactory> logger)
        {
            _localizationManager = localizationManager;
            _fallBackToParentCulture = requestLocalizationOptions.Value.FallBackToParentUICultures;
            _logger = logger;
        }

        /// <inheritedoc />
        public IStringLocalizer Create(Type resourceSource)
        {
            var resourceFullName = resourceSource.FullName;
            resourceFullName = TryFixInnerClassPath(resourceFullName);

            return new PortableObjectStringLocalizer(resourceFullName, _localizationManager, _fallBackToParentCulture, _logger);
        }

        /// <inheritedoc />
        public IStringLocalizer Create(string baseName, string location)
        {
            baseName = TryFixInnerClassPath(baseName);

            var index = 0;
            if (baseName.StartsWith(location, StringComparison.OrdinalIgnoreCase))
            {
                index = location.Length;
            }

            if (baseName.Length > index && baseName[index] == '.')
            {
                index += 1;
            }

            if (baseName.Length > index && baseName.IndexOf("Areas.", index, StringComparison.Ordinal) == index)
            {
                index += "Areas.".Length;
            }

            var relativeName = baseName[index..];

            return new PortableObjectStringLocalizer(relativeName, _localizationManager, _fallBackToParentCulture, _logger);
        }

        // The context within inner class.
        private static string TryFixInnerClassPath(string context)
        {
            const char innerClassSeparator = '+';
            var path = context;
            if (context.Contains(innerClassSeparator))
            {
                path = context.Replace(innerClassSeparator, '.');
            }

            return path;
        }
    }
}
