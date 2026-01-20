using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeDescriptorManager : IShortcodeDescriptorManager
    {
        private readonly IEnumerable<IShortcodeDescriptorProvider> _shortcodeDescriptorProviders;

        public ShortcodeDescriptorManager(IEnumerable<IShortcodeDescriptorProvider> shortcodeDescriptorProviders)
        {
            _shortcodeDescriptorProviders = shortcodeDescriptorProviders;
        }

        public async Task<IEnumerable<ShortcodeDescriptor>> GetShortcodeDescriptors()
        {
            var result = new Dictionary<string, ShortcodeDescriptor>(StringComparer.OrdinalIgnoreCase);

            // During discover providers are reversed so that first registered wins.
            // This allows the templates feature to override code based shortcodes.
            var reversedShortcodeDescriptorProviders = _shortcodeDescriptorProviders.Reverse();

            foreach (var provider in reversedShortcodeDescriptorProviders)
            {
                var descriptors = await provider.DiscoverAsync();
                foreach (var descriptor in descriptors)
                {
                    // Overwrite existing descriptors if they have been replaced.
                    result[descriptor.Name] = descriptor;
                }
            }

            return result.Values;
        }
    }
}
