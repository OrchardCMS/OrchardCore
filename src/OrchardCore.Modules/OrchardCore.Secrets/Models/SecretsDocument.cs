using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Models
{
    public class SecretsDocument
    {
        public Dictionary<string, DocumentSecret> Secrets { get; } = new Dictionary<string, DocumentSecret>(StringComparer.OrdinalIgnoreCase);
    }

    public class DocumentSecret
    {
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }
        public string Value { get; set; }
    }


}
