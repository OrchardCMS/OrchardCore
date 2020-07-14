using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeOptionsTableProvider : IShortcodeTableProvider
    {
        private ShortcodeOptions _options;

        public ShortcodeOptionsTableProvider(
            IOptions<ShortcodeOptions> options
            )
        {
            _options = options.Value;
        }

        public Task<IEnumerable<ShortcodeDescriptor>> DescribeAsync()
        {
            return Task.FromResult(_options.Shortcodes.Values);
        }
    }
}
