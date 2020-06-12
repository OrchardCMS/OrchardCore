using System.Collections.Generic;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.ShortCodes.Services
{
    public class ShortCodeService : IShortCodeService
    {
        private readonly ShortcodesProcessor _shortCodesProcessor;
        private readonly IEnumerable<IShortcodeProvider> _shortCodeProviders;

        public ShortCodeService(IEnumerable<IShortcodeProvider> shortCodeProviders)
        {
            _shortCodeProviders = shortCodeProviders;
            _shortCodesProcessor = new ShortcodesProcessor(shortCodeProviders);

        }

        public ValueTask<string> ProcessAsync(string input)
        {
            return _shortCodesProcessor.EvaluateAsync(input);
        }
    }
}
