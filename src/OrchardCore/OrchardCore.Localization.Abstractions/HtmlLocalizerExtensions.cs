using System;
using OrchardCore.Localization;

namespace Microsoft.AspNetCore.Mvc.Localization
{
    public static class HtmlLocalizerExtensions
    {
        public static LocalizedHtmlString Plural(this IHtmlLocalizer localizer, int count, string singular, string plural, params object[] arguments)
        {
            if (plural == null)
            {
                throw new ArgumentNullException(nameof(plural), "Plural text can't be null. If you don't want to specify the plural text, use IStringLocalizer without Plural extention.");
            }

            return localizer[singular, new PluralizationArgument { Count = count, Forms = new[] { singular, plural }, Arguments = arguments }];
        }

        public static LocalizedHtmlString Plural(this IHtmlLocalizer localizer, int count, string[] pluralForms, params object[] arguments)
        {
            if (pluralForms == null)
            {
                throw new ArgumentNullException(nameof(pluralForms), "PluralForms array can't be null. If you don't want to specify the plural text, use IStringLocalizer without Plural extention.");
            }

            if (pluralForms.Length == 0)
            {
                throw new ArgumentException(nameof(pluralForms), "PluralForms array can't be empty, it must contain at least one element. If you don't want to specify the plural text, use IStringLocalizer without Plural extention.");
            }

            var name = pluralForms[0];

            return localizer[name, new PluralizationArgument { Count = count, Forms = pluralForms, Arguments = arguments }];
        }
    }
}
