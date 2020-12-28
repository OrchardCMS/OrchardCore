using Newtonsoft.Json;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiTextFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string Options { get; set; }
    }

    public class MultiTextListValueOption
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }
    }
}
