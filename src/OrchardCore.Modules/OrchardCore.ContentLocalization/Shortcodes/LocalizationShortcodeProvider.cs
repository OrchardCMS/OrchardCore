using System;
using System.Globalization;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.ContentLocalization.Shortcodes
{
    public class LocalizationShortcodeProvider : IShortcodeProvider
    {
        public const string ShortCodeIdentifier = "locale";
        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            if (identifier != ShortCodeIdentifier)
            {
                return Null;
            }

            var language = arguments.NamedOrDefault("lang");
            var currentCulture = CultureInfo.CurrentCulture.Name;
            if(!string.Equals(language, currentCulture, StringComparison.InvariantCultureIgnoreCase))
            {
                return Null;
            }

            return new ValueTask<string>(content);
        }
    }
}
