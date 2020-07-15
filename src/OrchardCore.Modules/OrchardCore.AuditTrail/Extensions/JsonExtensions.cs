using Newtonsoft.Json.Linq;
using OrchardCore.AuditTrail.Services.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class JsonExtensions
    {
        public static List<DiffNode> GenerateDiffNodes(this JToken token, List<DiffNode> diffNodes = null)
        {
            if (diffNodes == null) diffNodes = new List<DiffNode>();

            var currentObject = token as JObject;
            var keys = currentObject.Properties();

            JToken previous = new JObject();
            JToken current = new JObject();

            foreach (var key in keys)
            {
                if (key.Name == "+")
                {
                    if (key.IsNotNullOrEmpty())
                    {
                        current = key.Value;
                    }
                }
                if (key.Name == "-")
                {
                    if (key.IsNotNullOrEmpty())
                    {
                        previous = key.Value;
                    }
                }

                if (current.Type != JTokenType.Object && previous.Type != JTokenType.Object)
                {
                    diffNodes.Add(new DiffNode 
                    { 
                        Type = DiffType.Change, 
                        Context = key.Parent.Path.Replace('.', '/'), 
                        Current = current, Previous = previous 
                    });
                }

                if (!key.Value.IsAllowedToCheckProperties())
                {
                    diffNodes = GenerateDiffNodes(currentObject[key.Name], diffNodes);
                }
            }

            return diffNodes;
        }

        public static JObject FindDiff(this JToken current, JToken previous)
        {
            var diff = new JObject();
            if (JToken.DeepEquals(current, previous)) return diff;

            switch (current.Type)
            {
                case JTokenType.Object:
                    {
                        var currentObject = current as JObject;
                        var previousObject = previous as JObject;

                        if (previousObject == null) break;

                        var addedKeys = currentObject.Properties()
                            .Select(property => property.Name).Except(previousObject.Properties()
                            .Select(property => property.Name));

                        var removedKeys = previousObject.Properties()
                            .Select(property => property.Name).Except(currentObject.Properties()
                            .Select(property => property.Name));

                        var unchangedKeys = currentObject.Properties()
                            .Where(property => JToken.DeepEquals(property.Value, previous[property.Name]))
                            .Select(property => property.Name);

                        foreach (var addedKey in addedKeys)
                        {
                            diff[addedKey] = new JObject
                            {
                                ["+"] = current[addedKey]
                            };
                        }

                        foreach (var removedKey in removedKeys)
                        {
                            diff[removedKey] = new JObject
                            {
                                ["-"] = previous[removedKey]
                            };
                        }

                        var potentiallyModifiedKeys = currentObject.Properties()
                            .Select(c => c.Name).Except(addedKeys).Except(unchangedKeys);

                        foreach (var potentiallyModifiedKey in potentiallyModifiedKeys)
                        {
                            diff[potentiallyModifiedKey] = FindDiff(currentObject[potentiallyModifiedKey], previousObject[potentiallyModifiedKey]);
                        }
                    }
                    break;
                case JTokenType.Array:
                    {
                        var currentArray = current as JArray;
                        var previousArray = previous as JArray;
                        diff["+"] = previousArray != null ? new JArray(currentArray.Except(previousArray)) : new JArray(currentArray);
                        diff["-"] = previousArray != null ? new JArray(previousArray.Except(currentArray)) : new JArray();
                    }
                    break;
                default:
                    diff["+"] = current;
                    diff["-"] = previous;
                    break;
            }

            return diff;
        }

        public static bool IsAllowedToCheckProperties(this JToken token) =>
            token == null ||
                token.Type == JTokenType.Array && token as JObject == null ||
                token.Type == JTokenType.Object && !token.HasValues ||
                token.Type == JTokenType.String ||
                token.Type == JTokenType.Null ||
                token.Type == JTokenType.Integer ||
                token.Type == JTokenType.Boolean;

        public static bool IsNotNullOrEmpty(this JProperty jProperty) =>
            jProperty.HasValues;
    }
}
