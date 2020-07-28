using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeOptionsTableProvider : IShortcodeTableProvider
    {
        private readonly ShortcodeOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public ShortcodeOptionsTableProvider(
            IOptions<ShortcodeOptions> options,
            IServiceProvider serviceProvider
            )
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
        }

        // public Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync()
        // {
            // var result = new List<ShortcodeDescriptor>();
            // foreach(var option in _options.Shortcodes.Values)
            // {
            //     var descriptor = new ShortcodeDescriptor
            //     {
            //         Name = option.Name,
            //         DefaultShortcode = option.DefaultShortcode,
            //         Usage = option.Usage
            //     };

            //     descriptor.Hint  = option.Hint.Invoke(_serviceProvider);
            //     descriptor.Categories = option.Categories.Invoke(_serviceProvider);
            //     result.Add(descriptor);
            // }

            // return Task.FromResult<IEnumerable<ShortcodeDescriptor>>(result);
        // }

        public Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync() =>
            Task.FromResult(_options.Shortcodes.Values.Select(option =>
                new ShortcodeDescriptor
                {
                    Name = option.Name,
                    DefaultShortcode = option.DefaultShortcode,
                    Usage = option.Usage,
                    Hint = option.Hint.Invoke(_serviceProvider),
                    Categories = option.Categories.Invoke(_serviceProvider)
                }));
    }
}
