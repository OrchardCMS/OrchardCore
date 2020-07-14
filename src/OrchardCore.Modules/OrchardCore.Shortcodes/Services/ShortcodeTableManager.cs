using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeTableManager : IShortcodeTableManager
    {
        private readonly IEnumerable<IShortcodeTableProvider> _shortcodeTableProviders;

        public ShortcodeTableManager(
            IEnumerable<IShortcodeTableProvider> shortcodeTableProviders
            )
        {
            _shortcodeTableProviders = shortcodeTableProviders;
        }

        public async Task<IEnumerable<ShortcodeDescriptor>> BuildAsync()
        {
            var result = new Dictionary<string, ShortcodeDescriptor>(StringComparer.OrdinalIgnoreCase);

            // During discover providers are reversed so that first registered wins.
            // This allows the templates feature to override code based shortcodes.
            var reversedShortcodeTableProviders = _shortcodeTableProviders.Reverse();

            foreach(var provider in reversedShortcodeTableProviders)
            {
                var descriptors = await provider.DescribeAsync();
                foreach(var descriptor in descriptors)
                {
                    // Overwrite existing descriptors if they have been replaced.
                    result[descriptor.Name] = descriptor;
                }
            }

            return result.Values;
        }
    }
}
