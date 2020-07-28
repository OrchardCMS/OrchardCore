using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeTemplatesTableProvider : IShortcodeTableProvider
    {
        private readonly ShortcodeTemplatesManager _shortcodeTemplatesManager;

        public ShortcodeTemplatesTableProvider(
            ShortcodeTemplatesManager shortcodeTemplatesManager)
        {
            _shortcodeTemplatesManager = shortcodeTemplatesManager;
        }

        public async Task<IEnumerable<ShortcodeDescriptor>> DiscoverAsync()
        {
            var document = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

            return document.ShortcodeTemplates.Select(kvp =>
                new ShortcodeDescriptor
                {
                    Name = kvp.Key,
                    Hint = kvp.Value.Hint,
                    DefaultShortcode = kvp.Value.DefaultShortcode,
                    Usage = kvp.Value.Usage,
                    Categories = kvp.Value.Categories
                });
            }
    }
}
