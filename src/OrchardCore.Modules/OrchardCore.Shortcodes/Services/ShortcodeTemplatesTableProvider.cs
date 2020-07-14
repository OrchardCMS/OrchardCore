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

        public async Task<IEnumerable<ShortcodeDescriptor>> DescribeAsync()
        {
            var document = await _shortcodeTemplatesManager.GetShortcodeTemplatesDocumentAsync();

            return document.ShortcodeTemplates.Select(kvp =>
                new ShortcodeDescriptor
                {
                    Name = kvp.Key,
                    Hint = kvp.Value.Description, // TODO if we keep the hint concept, rename in templates, and provide more meta there
                    Categories = new string[] { "HTML Content" } // including categories. Content by default? probably not actually.
                });
            }
    }
}
