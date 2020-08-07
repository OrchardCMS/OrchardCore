using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Models
{
    public class SecretBindingsDocument
    {
        public Dictionary<string, SecretBinding> SecretBindings { get; } = new Dictionary<string, SecretBinding>(StringComparer.OrdinalIgnoreCase);
    }

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
