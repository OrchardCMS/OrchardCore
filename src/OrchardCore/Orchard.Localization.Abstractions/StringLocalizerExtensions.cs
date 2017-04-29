using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization.Abstractions
{
    public static class StringLocalizerExtensions
    {
        public static LocalizedString Plural(this IStringLocalizer localizer, string name, string pluralText, int count, params object[] arguments)
        {
            if(pluralText == null)
            {
                throw new ArgumentNullException(nameof(pluralText), "Plural text can't be null. If you don't want to specify the plural text, use IStringLocalizer without Plural extention.");
            }

            return localizer[name, new object[] { new PluralArgument() { PluralText = pluralText, Count = count }, arguments }];
        }
    }
}
