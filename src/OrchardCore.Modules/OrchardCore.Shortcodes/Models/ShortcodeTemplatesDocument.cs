using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.Shortcodes.Models;

public class ShortcodeTemplatesDocument : Document
{
    private readonly Dictionary<string, ShortcodeTemplate> _shortcodeTemplates = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, ShortcodeTemplate> ShortcodeTemplates
    {
        get => _shortcodeTemplates;
        set => _shortcodeTemplates.SetItems(value);
    }
}

public class ShortcodeTemplate
{
    public string Content { get; set; }
    public string Hint { get; set; }
    public string Usage { get; set; }
    public string DefaultValue { get; set; }
    public string[] Categories { get; set; } = [];
}
