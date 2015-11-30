using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement
{
    public class ContentPart : ContentElement
    {
        public ContentPart() : base()
        {
        }

        [JsonIgnore()]
        internal override JObject Data { get; set; }
    }
}