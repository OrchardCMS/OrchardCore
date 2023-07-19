using System;
using System.Globalization;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.Shortcodes.Providers
{
    public class LocaleShortcodeProvider : IShortcodeProvider
    {
        public const string ShortCodeIdentifier = "locale";

        private static ValueTask<string> Empty => new(String.Empty);
        private static ValueTask<string> Null => new((string)null);

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            if (identifier != ShortCodeIdentifier)
            {
                return Null;
            }

            var language = arguments.NamedOrDefault("lang")?.ToLower();
            var argFallback = arguments.NamedOrAt("fallback", 1);

            // Default value of true for the fallback argument.
            var fallback = argFallback == null || Convert.ToBoolean(argFallback);
            var currentCulture = CultureInfo.CurrentUICulture;

            if (fallback)
            {
                // Fallback to parent culture, if the current culture is en-CA and the shortcode targets en, the html will be output.
                do
                {
                    if (currentCulture.Name.Equals(language, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new ValueTask<string>(content);
                    }

                    currentCulture = currentCulture.Parent;
                }
                while (currentCulture != CultureInfo.InvariantCulture);
            }
            else
            {
                if (currentCulture.Name.Equals(language, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new ValueTask<string>(content);
                }
            }

            return Empty;
        }
    }
}
