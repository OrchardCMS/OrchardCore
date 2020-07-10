using System.Collections.Generic;
using OrchardCore.Shortcodes.Models;

namespace OrchardCore.Shortcodes.ViewModels
{
    public class ShortcodeTemplateIndexViewModel
    {
        public IList<ShortcodeTemplateEntry> ShortcodeTemplates { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ShortcodeTemplateEntry
    {
        public string Name { get; set; }
        public ShortcodeTemplate ShortcodeTemplate { get; set; }
        public bool IsChecked { get; set; }
    }
}
