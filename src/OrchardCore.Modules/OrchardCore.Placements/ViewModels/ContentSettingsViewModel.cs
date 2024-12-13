using Microsoft.Extensions.Localization;

namespace OrchardCore.Placements.ViewModels;

public class ContentSettingsViewModel
{
    public List<ContentSettingsEntry> ContentSettingsEntries { get; set; } = [];
}

public class ContentSettingsEntry
{
    public string ShapeType { get; set; }
    public string DisplayType { get; set; }
    public string ContentType { get; set; }
    public string ContentPart { get; set; }
    public string Differentiator { get; set; }
    public LocalizedString Description { get; set; }
}
