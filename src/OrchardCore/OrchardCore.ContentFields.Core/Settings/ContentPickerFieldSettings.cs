using System.ComponentModel;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class ContentPickerFieldSettings : FieldSettings
{
    [Description("Allows multiple ContentItemIds. Defaults to false; more than one item fails content validation unless enabled.")]
    public bool Multiple { get; set; }

    [Description("Shows all content types without a stereotype in the default editor picker. Defaults to false.")]
    public bool DisplayAllContentTypes { get; set; }

    [Description("Technical content type names offered by the editor picker when DisplayAllContentTypes is false and DisplayedStereotypes is empty. Defaults to an empty list; this is an editor filter, not a content validation rule.")]
    public string[] DisplayedContentTypes { get; set; } = [];

    [Description("Stereotypes used to select the editor picker's content types. Takes precedence over DisplayedContentTypes when nonempty.")]
    public string[] DisplayedStereotypes { get; set; } = [];

    public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// The Liquid pattern used to build the title.
    /// </summary>
    [DefaultValue("{{ Model.ContentItem | display_text }}")]
    public string TitlePattern { get; set; } = "{{ Model.ContentItem | display_text }}";
}
