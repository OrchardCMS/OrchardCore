using System.Collections.Generic;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeService : IShortcodeService
    {
        private readonly ShortcodesProcessor _shortcodesProcessor;
        private readonly IEnumerable<IShortcodeProvider> _shortcodeProviders;

        public ShortcodeService(IEnumerable<IShortcodeProvider> shortcodeProviders)
        {
            _shortcodeProviders = shortcodeProviders;
            _shortcodesProcessor = new ShortcodesProcessor(shortcodeProviders);

        }

        public ValueTask<string> ProcessAsync(string input)
        {
            return _shortcodesProcessor.EvaluateAsync(input);
        }
    }
}
