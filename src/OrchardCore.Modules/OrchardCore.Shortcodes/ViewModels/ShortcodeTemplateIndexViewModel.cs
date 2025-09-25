using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Shortcodes.Models;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.ViewModels;

public class ShortcodeTemplateIndexViewModel
{
    public ShortcodeFilter Filter { get; }

    public ContentOptionsViewModel Options { get; set; }

    [BindNever]
    public IList<ShortcodeTemplateEntry> ShortcodeTemplates { get; set; }

    [BindNever]
    public dynamic Header { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }
}

public class ShortcodeTemplateEntry
{
    public string Name { get; set; }
    public ShortcodeTemplate ShortcodeTemplate { get; set; }
    public bool IsChecked { get; set; }
}
