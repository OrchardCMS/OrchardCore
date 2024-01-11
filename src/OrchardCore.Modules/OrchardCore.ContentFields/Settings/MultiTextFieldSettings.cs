using System;
using Newtonsoft.Json;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiTextFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public MultiTextFieldValueOption[] Options { get; set; } = Array.Empty<MultiTextFieldValueOption>();
    }

    public class MultiTextFieldValueOption
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }
    }
}
