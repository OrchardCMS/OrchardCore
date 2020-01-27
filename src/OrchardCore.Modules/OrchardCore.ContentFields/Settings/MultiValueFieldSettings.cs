using Newtonsoft.Json;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiValueFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string Options { get; set; }
        public MultiValueEditorOption Editor { get; set; }
    }
    public enum MultiValueEditorOption
    {
        Listbox,
        Checkboxes
    }

    public class MultiValueListValueOption
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}