using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Shortcodes.Services
{
    public class ShortcodeTemplatesDescriptorProvider : IShortcodeDescriptorProvider
    {
        private readonly ShortcodeTemplatesManager _shortcodeTemplatesManager;

        public ShortcodeTemplatesDescriptorProvider(ShortcodeTemplatesManager shortcodeTemplatesManager)
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
                    DefaultValue = kvp.Value.DefaultValue,
                    Usage = kvp.Value.Usage,
                    Categories = kvp.Value.Categories
                });
        }
    }
}
