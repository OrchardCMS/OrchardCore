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

    public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// The minimum number of characters allowed. A value of <c>0</c> means no minimum.
    /// </summary>
    public int MinLength { get; set; }

    /// <summary>
    /// The maximum number of characters allowed. A value of <c>0</c> means no maximum.
    /// </summary>
    public int MaxLength { get; set; }
}
