using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace OrchardCore.Localization
{
    public class DataResourceManager : ResourceManager
    {
        private readonly IEnumerable<CultureDictionary> _innerDictionaries;
        private readonly bool _fallBackToParentCulture;

        public DataResourceManager(IEnumerable<CultureDictionary> resourceDictionary, bool fallBackToParentCulture = true)
        {
            _innerDictionaries = resourceDictionary ?? throw new ArgumentNullException(nameof(resourceDictionary));
            _fallBackToParentCulture = fallBackToParentCulture;
        }

        public override string GetString(string name) => GetString(name, CultureInfo.CurrentUICulture);

        public override string GetString(string name, CultureInfo culture)
        {
            if (_fallBackToParentCulture)
            {
                var currentCulture = culture;
                string value = null;

                do
                {
                    value = GetStringInternal(name, currentCulture);

                    if (value != null)
                    {
                        break;
                    }

                    currentCulture = currentCulture.Parent;
                }
                while (currentCulture != currentCulture.Parent);

                return value;
            }
            else
            {
                return GetStringInternal(name, culture);
            }
        }

        public IEnumerable<string> GetAllResourceStrings(CultureInfo culture, bool tryParents = false)
        {
            if (tryParents)
            {
                var currentCulture = culture;
                var resourceNames = new HashSet<string>();

                do
                {
                    var cultureResourceNames = GetAllResourceStringsInternal(currentCulture);

                    if (resourceNames != null)
                    {
                        foreach (var resourceName in cultureResourceNames)
                        {
                            resourceNames.Add(resourceName);
                        }
                    }

                    currentCulture = currentCulture.Parent;
                }
                while (currentCulture != currentCulture.Parent);

                return resourceNames;
            }
            else
            {
                return GetAllResourceStringsInternal(culture);
            }
        }

        private IEnumerable<string> GetAllResourceStringsInternal(CultureInfo culture)
        {
            var dictionary = _innerDictionaries.SingleOrDefault(d => d.CultureName == culture.Name);

            if (dictionary == null)
            {
                yield return null;
            }

            foreach (var item in dictionary.Translations)
            {
                yield return item.Key;
            }
        }

        private string GetStringInternal(string name, CultureInfo culture)
        {
            var dictionary = _innerDictionaries.SingleOrDefault(d => d.CultureName == culture.Name);

            if (dictionary == null)
            {
                return null;
            }

            return dictionary.Translations.ContainsKey(name)
                ? dictionary.Translations[name].First()
                : null;
        }
    }
}