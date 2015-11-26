using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement
{
    public class ContentField : ContentElement
    {
        [JsonIgnore]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }

        [JsonIgnore()]
        internal override JObject Data { get; set; }
    }
}