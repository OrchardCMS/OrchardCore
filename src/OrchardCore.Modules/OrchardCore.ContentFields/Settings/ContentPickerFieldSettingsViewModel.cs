using System;

namespace OrchardCore.ContentFields.Settings;

public class ContentPickerFieldSettingsViewModel
{
    public string Hint { get; set; }

    public ContentPickerSettingType Source { get; set; }

    public string Stereotypes { get; set; }

    public bool Required { get; set; }

    public bool Multiple { get; set; }

    public string[] DisplayedContentTypes { get; set; } = Array.Empty<string>();

    public string TitlePattern { get; set; } = "{{ Model.ContentItem.DisplayText }}";
}
