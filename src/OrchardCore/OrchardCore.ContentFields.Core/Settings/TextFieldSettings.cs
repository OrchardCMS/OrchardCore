using System.Text.Json.Serialization;
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
    /// Gets or sets the minimum number of characters allowed. A value of <c>null</c> means no minimum.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of characters allowed. A value of <c>null</c> means no maximum.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? MaxLength { get; set; }
}
