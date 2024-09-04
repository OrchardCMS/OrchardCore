using System.Text.Json.Serialization;

namespace OrchardCore.ContentFields.Settings;

public class TextFieldPredefinedListEditorSettings
{
    public ListValueOption[] Options { get; set; }
    public EditorOption Editor { get; set; }
    public string DefaultValue { get; set; }
}

public enum EditorOption
{
    Radio,
    Dropdown
}

public class ListValueOption
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    public ListValueOption()
    {
    }

    public ListValueOption(string name) : this(name, name)
    {
    }

    public ListValueOption(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
