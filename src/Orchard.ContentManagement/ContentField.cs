using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement
{
    public class ContentField : ContentElement
    {
        [JsonIgnore()]
        internal override JObject Data { get; set; }
    }
}