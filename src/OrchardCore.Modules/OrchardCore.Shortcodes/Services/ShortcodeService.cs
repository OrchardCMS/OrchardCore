using System.Collections.Generic;
using System.Threading.Tasks;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeService : IShortcodeService
    {
        private readonly IEnumerable<IShortcodeContextProvider> _shortcodeContextProviders;
        private readonly ShortcodesProcessor _shortcodesProcessor;

        public ShortcodeService(
            IEnumerable<IShortcodeProvider> shortcodeProviders,
            IEnumerable<IShortcodeContextProvider> shortcodeContextProviders)
        {
            _shortcodesProcessor = new ShortcodesProcessor(shortcodeProviders);
            _shortcodeContextProviders = shortcodeContextProviders;
        }

        public ValueTask<string> ProcessAsync(string input, Context context = null)
        {
            context ??= new Context();

            foreach (var contextProvider in _shortcodeContextProviders)
            {
                contextProvider.Contextualize(context);
            }

            return _shortcodesProcessor.EvaluateAsync(input, context);
        }
    }
}
