using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization
{
    /*
    {
     { "blog" :
        [
            { "" : "Blog"},
            { "fr" : "Blog"},
            { "ar" : "مدونة"}
        ]
     }
    */
    public class ContentTypeDefinitionDataLocalizer : IDataLocalizer
    {
        private readonly CultureDictionary _data;
        private readonly ILogger _logger;

        public ContentTypeDefinitionDataLocalizer(CultureDictionary data, ILogger logger)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
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
            foreach (var translation in _data.Translations)
            {
                yield return new LocalizedString(translation.Key, translation.Value.First());
            }
        }

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        private string GetTranslation(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            string translation = null;

            if (_data.Translations.ContainsKey(name))
            {
                translation = _data.Translations[name].First();
            }

            //TODO: Log the content type definition name for the resource that we looking for
            _logger.LogDebug($"{nameof(ContentTypeDefinitionDataLocalizer)} searched for '{name}' in the database with culture '{CultureInfo.CurrentUICulture.Name}'.");

            return translation;
        }
    }
}