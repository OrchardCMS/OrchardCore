using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Localization.DataAnnotations;

namespace OrchardCore.Localization.Extensions
{
    internal static class CultureDictionaryExtensions
    {
        private static readonly string DataAnnotationsContext = typeof(DataAnnotationsDefaultErrorMessages).FullName;

        public static IEnumerable<string> GetDataAnnotationKeys(this CultureDictionary cultureDictionary)
        {
            if (cultureDictionary is null)
            {
                throw new ArgumentNullException(nameof(cultureDictionary));
            }

            return cultureDictionary.Translations
                .Where(t => t.Key.ToString().StartsWith(DataAnnotationsContext, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Key.ToString())
                .ToList();
        }
    }
}
