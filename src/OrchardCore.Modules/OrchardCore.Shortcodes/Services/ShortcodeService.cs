using System.Collections.Generic;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeService : IShortcodeService
    {
        private readonly ShortcodesProcessor _shortCodesProcessor;
        private readonly IEnumerable<IShortcodeProvider> _shortCodeProviders;

        public ShortcodeService(IEnumerable<IShortcodeProvider> shortCodeProviders)
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
