using Microsoft.Extensions.Localization;

namespace OrchardCore.Templates.ViewModels;

public class ContentSettingsViewModel
{
    public List<ContentSettingsEntry> ContentSettingsEntries { get; set; } = [];
}

public class ContentSettingsEntry
{
    public string Key { get; set; }
    public LocalizedString Description { get; set; }
}
