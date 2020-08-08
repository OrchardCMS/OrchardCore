using Newtonsoft.Json;

namespace OrchardCore.Secrets
{
    public class SecretBinding
    {
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

        public string Store { get; set; }
        //TODO remove
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
