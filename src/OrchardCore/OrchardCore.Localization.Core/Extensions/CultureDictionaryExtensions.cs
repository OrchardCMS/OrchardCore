using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Localization.Core.DataAnnotations;

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
                .Where(t => t.Key.StartsWith(DataAnnotationsContext, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Key)
                .ToList();
        }
    }
}
