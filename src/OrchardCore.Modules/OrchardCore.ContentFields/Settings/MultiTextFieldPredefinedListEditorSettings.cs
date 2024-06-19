using System.Text.Json.Serialization;

namespace OrchardCore.ContentFields.Settings;

public class MultiTextFieldPredefinedListEditorSettings
{
    public MultiTextFieldValueOption[] Options { get; set; } = [];
}

public class MultiTextFieldValueOption
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("default")]
    public bool Default { get; set; }
}
