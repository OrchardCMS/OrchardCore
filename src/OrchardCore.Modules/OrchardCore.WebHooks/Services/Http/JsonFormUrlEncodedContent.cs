using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OrchardCore.WebHooks.Services.Http {

    /// <summary>
    /// Produces form-urlencoded content by flattening a provided JSON object. 
    /// </summary>
    public class JsonFormUrlEncodedContent : FormUrlEncodedContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFormUrlEncodedContent" /> class with a JSON onbject.
        /// </summary>
        /// <param name="input">The JSON object to be output as form url encoded content.</param>
        public JsonFormUrlEncodedContent(JObject input) : base(FlattenToNameValueCollection(input))
        {
        }

        private static IEnumerable<KeyValuePair<string, string>> FlattenToNameValueCollection(JObject input)
        {
            var pairs = new List<KeyValuePair<string, string>>();
            
            var stack = new List<object>();
            foreach (var property in input)
            {
                stack.Add(property.Key);
                Flatten(pairs, property.Value, stack);
                stack.RemoveAt(stack.Count - 1);
                if (stack.Count != 0)
                {
                    throw new InvalidOperationException("Something went wrong flattening json object.");
                }
            }

            return pairs;
        }

        private static void Flatten(List<KeyValuePair<string, string>> pairs, JToken input, List<object> indices)
        {
            if (input == null)
            {
                return;  
            }

            switch (input.Type)
            {
                case JTokenType.Null:
                {
                    return; // null values aren't serialized
                }
                case JTokenType.Array:
                    for (int i = 0; i < input.Count(); i++)
                    {
                        indices.Add(i);
                        Flatten(pairs, input[i], indices);
                        indices.RemoveAt(indices.Count - 1);
                    }

                    break;
                case JTokenType.Object:
                    foreach (var kvp in input.Value<JObject>())
                    {
                        indices.Add(kvp.Key);
                        Flatten(pairs, kvp.Value, indices);
                        indices.RemoveAt(indices.Count - 1);
                    }

                    break;
                default:
                    var name = new StringBuilder();
                    for (int i = 0; i < indices.Count; i++)
                    {
                        var index = indices[i];
                        if (i > 0)
                        {
                            name.Append('[');
                        }

                        if (i < indices.Count - 1 || index is string)
                        {
                            // last array index not shown 
                            name.Append(index);
                        }

                        if (i > 0)
                        {
                            name.Append(']');
                        }
                    }

                    pairs.Add(new KeyValuePair<string, string>(name.ToString(), input.Value<string>()));
                    break;
            }
        }
    }
}