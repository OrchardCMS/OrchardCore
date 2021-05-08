using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class JsonExtensions
    {
        public static readonly JsonMergeSettings JsonMergeSettings = new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Merge };

        public static DiffNode[] GenerateDiffNodes(this JToken diff, string root = "")
        {
            if (diff.Type == JTokenType.Object || diff.Type == JTokenType.Array)
            {
                return ((JContainer)diff).DescendantsAndSelf()
                .Where(token => token.Type == JTokenType.Object && ((JObject)token).ContainsKey("+"))
                .Select(token => new DiffNode
                {
                    Type = DiffType.Change,
                    Context = root + "/" + token.Path.Replace('.', '/'),
                    Current = token["+"],
                    Previous = token["-"]
                })
                .ToArray();
            }

            if (diff is JProperty property)
            {
                return GenerateDiffNodes(property.Value, root);
            }

            return null;
        }

        public static bool FindDiff(this JToken current, JToken previous, out JToken diff)
        {
            if (current == null || previous == null || current.Type != previous.Type)
            {
                diff = null;
                return false;
            }

            if (current.Type == JTokenType.Object || current.Type == JTokenType.Array)
            {
                var schema = current.CreateNull() as JContainer;
                schema.Merge(previous.CreateNull(), JsonMergeSettings);

                JContainer currentContainer, previousContainer;
                if (current.Type == JTokenType.Object)
                {
                    currentContainer = new JObject((JObject)schema);
                    previousContainer = new JObject((JObject)schema);
                }
                else
                {
                    currentContainer = new JArray((JArray)schema);
                    previousContainer = new JArray((JArray)schema);
                }

                currentContainer.Merge(current, JsonMergeSettings);
                previousContainer.Merge(previous, JsonMergeSettings);

                if (FindDiffInternal(currentContainer, previousContainer, out diff))
                {
                    return true;
                }
            }

            else if (FindDiffInternal(current, previous, out diff))
            {
                return true;
            }

            return false;
        }

        internal static bool FindDiffInternal(this JToken current, JToken previous, out JToken diff)
        {
            if (JToken.DeepEquals(current, previous))
            {
                diff = null;
                return false;
            }

            if (current.Type == JTokenType.Object || current.Type == JTokenType.Array)
            {
                var jContainer = current.Type == JTokenType.Object ? new JObject() : new JArray() as JContainer;
                for (var i = 0; i < current.Children().Count(); i++)
                {
                    if (FindDiffInternal(current.ElementAt(i), previous.ElementAt(i), out var jToken))
                    {
                        jContainer.Add(jToken);
                    }
                }

                diff = jContainer;
            }

            else if (current is JProperty property)
            {
                var jProperty = new JProperty(new JProperty(property.Name));
                if (FindDiffInternal(property.Value, ((JProperty)previous).Value, out var jToken))
                {
                    jProperty.Value = jToken;
                }

                diff = jProperty;
            }
            else
            {
                diff = new JObject
                {
                    { "+", current },
                    { "-", previous }
                };
            }

            return true;
        }

        public static JToken CreateNull(this JToken jToken)
        {
            if (jToken.Type == JTokenType.Object)
            {
                var jObject = new JObject();
                foreach (var child in jToken.Children())
                {
                    jObject.Add(CreateNull(child));
                }

                return jObject;
            }

            if (jToken.Type == JTokenType.Array)
            {
                var jArray = new JArray();
                foreach (var child in jToken.Children())
                {
                    jArray.Add(CreateNull(child));
                }

                return jArray;
            }

            if (jToken is JProperty jProperty)
            {
                return new JProperty(jProperty.Name, CreateNull(jProperty.Value));
            }

            return jToken.Type == JTokenType.String ? new JValue(String.Empty) : JValue.CreateNull();
        }
    }
}
