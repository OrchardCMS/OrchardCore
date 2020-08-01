using System.Collections.Generic;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeService : IShortcodeService
    {
        private readonly ShortcodesProcessor _shortcodesProcessor;

        public ShortcodeService(IEnumerable<IShortcodeProvider> shortcodeProviders)
        {
            _shortcodesProcessor = new ShortcodesProcessor(shortcodeProviders);
        }

        public ValueTask<string> ProcessAsync(string input)
        {
            return _shortcodesProcessor.EvaluateAsync(input);
        }
    }
}
