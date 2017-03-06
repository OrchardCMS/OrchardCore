using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Common traits of <see cref="ContentItem"/>, <see cref="ContentPart"/>
    /// and <see cref="ContentField"/>
    /// </summary>
    public class ContentElement : IContent
    {
        protected ContentElement() : this(new JObject())
        {
        }

        protected ContentElement(JObject data)
        {
            Data = data;
        }

        [JsonIgnore]
        public dynamic Content { get { return Data; } }

        [JsonIgnore]
        internal JObject Data { get; set; }

        [JsonIgnore]
        public ContentItem ContentItem { get; set; }

        /// <summary>
        /// Whether the content has a named property or not.
        /// </summary>
        /// <param name="name">The name of the property to look for.</param>
        public bool Has(string name)
        {
            JToken value;
            return Data.TryGetValue(name, out value);
        }        
    }
}