using System.Text.Json.Serialization;

namespace OrchardCore.ContentFields.Settings;

public class MultiTextFieldSettings
{
    public string Hint { get; set; }
    public bool Required { get; set; }
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
