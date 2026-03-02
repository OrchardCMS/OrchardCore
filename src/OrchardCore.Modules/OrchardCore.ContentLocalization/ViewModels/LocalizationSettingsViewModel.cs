using OrchardCore.ContentLocalization.Models;

namespace OrchardCore.ContentLocalization.ViewModels;

public class LocalizationSettingsViewModel
{
    public string[] SelectedContentTypes { get; set; } = [];

    public IList<ContentTypeEntry> ContentTypeEntries { get; set; } = new List<ContentTypeEntry>();
}
