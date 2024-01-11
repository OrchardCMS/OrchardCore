using Newtonsoft.Json;

namespace OrchardCore.ContentFields.Settings
{
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
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
