using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization
{
    public class ContentTypeDefinitionDataLocalizer : IDataLocalizer
    {
        private readonly DataResourceManager _resourceManager;
        private readonly ILogger _logger;

        public ContentTypeDefinitionDataLocalizer(DataResourceManager resourceManager, ILogger logger)
        {
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetTranslation(name);

                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = GetTranslation(name);
                var value = String.Format(format ?? name, arguments);

                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var resourceNames = _resourceManager.GetAllResourceStrings(CultureInfo.CurrentUICulture, includeParentCultures);

            foreach (var name in resourceNames)
            {
                var value = GetTranslation(name);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        private string GetTranslation(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var translation = _resourceManager.GetString(name);

            //TODO: Log the content type definition name for the resource that we looking for
            _logger.LogDebug($"{nameof(ContentTypeDefinitionDataLocalizer)} searched for '{name}' in the database with culture '{CultureInfo.CurrentUICulture.Name}'.");

            return translation;
        }
    }
}