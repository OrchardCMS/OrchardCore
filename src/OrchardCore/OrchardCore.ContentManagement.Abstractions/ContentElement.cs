using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public bool TryGetPropertyValue(string propertyName, out JsonNode jsonNode) =>
            Data.TryGetPropertyValue(propertyName, out jsonNode);

        /// <summary>
        /// Tries to access a property in the <see cref="Content"/> by name and deserialize it as an array.
        /// </summary>
        /// <param name="propertyName">The name of the property to look for.</param>
        /// <returns>The deserialized array or an empty array if the property is not found or isn't an array.</returns>
        public TItem[] GetArrayProperty<TItem>(string propertyName)
        {
            return TryGetPropertyValue(propertyName, out var node) && node is JsonArray array
                ? array.Deserialize<TItem[]>()
                : Array.Empty<TItem>();
        }

        /// <summary>
        /// Serialized <paramref name="value"/> and then sets the value of the property.
        /// </summary>
        public void SetProperty<TValue>(string propertyName, TValue value) =>
            Data[propertyName] = JsonSerializer.SerializeToNode(value);
    }
}
