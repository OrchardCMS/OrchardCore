
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public class OptionsShortcodeProvider : IShortcodeProvider
    {
        private static ValueTask<string> Null => new((string)null);

        private readonly ShortcodeOptions _options;

        public OptionsShortcodeProvider(IOptions<ShortcodeOptions> options)
        {
            _options = options.Value;
        }

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            if (_options.ShortcodeDelegates.TryGetValue(identifier, out var shortcode))
            {
                if (shortcode == null)
                {
                    return Null;
                }

                return shortcode.Invoke(arguments, content, context);
            }

            return Null;
        }
    }
}
