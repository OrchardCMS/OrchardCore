using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Common traits of <see cref="ContentItem"/>, <see cref="ContentPart"/>
    /// and <see cref="ContentField"/>
    /// </summary>
    public class ContentElement : IContent
    {
        private Dictionary<string, ContentElement> _elements;

        protected ContentElement() : this(new JsonObject())
        {
        }

        protected ContentElement(JsonObject data)
        {
            Data = data;
        }

        [JsonIgnore]
        protected internal Dictionary<string, ContentElement> Elements =>
            _elements ??= new Dictionary<string, ContentElement>();

        [JsonIgnore]
        public dynamic Content => Data;

        [JsonIgnore]
        internal JsonObject Data { get; set; }

        [JsonIgnore]
        public ContentItem ContentItem { get; set; }

        /// <summary>
        /// Whether the content has a named property or not.
        /// </summary>
        /// <param name="name">The name of the property to look for.</param>
        public bool Has(string name)
        {
            return Data.ContainsKey(name);
        }

        /// <summary>
        /// Tries to access a property in the <see cref="Content"/> by name.
        /// </summary>
        /// <param name="propertyName">The name of the property to look for.</param>
        /// <param name="jsonNode">The property value node.</param>
        /// <returns></returns>
        public bool TryGetPropertyValue(string propertyName, out JsonNode jsonNode) =>
            Data.TryGetPropertyValue(propertyName, out jsonNode);
    }
}
