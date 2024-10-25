using System.ComponentModel;

namespace OrchardCore.ContentFields.Settings;

public class ContentPickerFieldSettings
{
    public string Hint { get; set; }

    public bool Required { get; set; }

    public bool Multiple { get; set; }

    public bool DisplayAllContentTypes { get; set; }

    public string[] DisplayedContentTypes { get; set; } = [];

    public string[] DisplayedStereotypes { get; set; } = [];

    /// <summary>
    /// The Liquid pattern used to build the title.
    /// </summary>
    [DefaultValue("{{ Model.ContentItem | display_text }}")]
    public string TitlePattern { get; set; } = "{{ Model.ContentItem | display_text }}";
}
