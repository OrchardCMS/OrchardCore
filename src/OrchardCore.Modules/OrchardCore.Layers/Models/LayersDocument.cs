using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Layers.Models
{
    public class LayersDocument
    {
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

        public List<Layer> Layers { get; set; } = new List<Layer>();
    }
}
