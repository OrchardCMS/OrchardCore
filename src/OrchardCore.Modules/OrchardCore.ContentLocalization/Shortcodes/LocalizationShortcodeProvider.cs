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

            var language = arguments.NamedOrDefault("lang")?.ToLower();
            var argFallback = arguments.NamedOrAt("fallback", 1);
            // default value of true for the fallback
            var fallback = argFallback == null ? true : Convert.ToBoolean(argFallback); ;
            var currentCulture = CultureInfo.CurrentCulture;

            if (fallback)
            {
                // fallback to parent culture, if the current culture is en-CA and the shortcode targets en, the html will be output
                do
                {
                    if (currentCulture.Name.Equals(language, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return new ValueTask<string>(content);
                    }

                    currentCulture = currentCulture.Parent;
                }
                while (currentCulture != CultureInfo.InvariantCulture);
            }
            else
            {
                if (currentCulture.Name.Equals(language, StringComparison.CurrentCultureIgnoreCase))
                {
                    return new ValueTask<string>(content);
                }
            }

            return Null;
        

        }
    }
}
