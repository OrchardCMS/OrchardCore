using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class TextFieldSettings : FieldSettings
{
    public string DefaultValue { get; set; }

    public FieldBehaviorType Type { get; set; }

    /// <summary>
    /// The pattern used to build the value.
    /// </summary>
    public string Pattern { get; set; }
}
