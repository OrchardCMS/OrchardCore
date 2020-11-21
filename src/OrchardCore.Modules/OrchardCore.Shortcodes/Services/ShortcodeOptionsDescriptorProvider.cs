using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeOptionsDescriptorProvider : IShortcodeDescriptorProvider
    {
        private readonly ShortcodeOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public ShortcodeOptionsDescriptorProvider(
            IOptions<ShortcodeOptions> options,
            IServiceProvider serviceProvider
            )
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        public Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync() =>
            Task.FromResult(_options.Shortcodes.Values.Select(option =>
                new ShortcodeDescriptor
                {
                    Name = option.Name,
                    DefaultValue = option.DefaultValue,
                    Usage = option.Usage,
                    Hint = option.Hint,
                    Categories = option.Categories
                }));
    }
}
