using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Models
{
    public class RecipeExecutionContext
    {
        public string ExecutionId { get; set; }
        public object Environment { get; set; }
        public string Name { get; set; }
        public JsonObject Step { get; set; }
        public RecipeDescriptor RecipeDescriptor { get; set; }
        public IEnumerable<RecipeDescriptor> InnerRecipes { get; set; }

        public T GetStep<T>() => Step.Deserialize<T>();

        public bool TryGetStepPropertyIfNameMatches<TValue>(
            string propertyName,
            out IEnumerable<KeyValuePair<string, TValue>> properties)
        {
            if (!string.Equals(Name, propertyName, StringComparison.OrdinalIgnoreCase) ||
                !Step.TryGetPropertyValue(propertyName, out var node) ||
                node is not JsonObject jsonObject)
            {
                properties = Enumerable.Empty<KeyValuePair<string, TValue>>();
                return false;
            }

            properties = jsonObject.Select(property => new KeyValuePair<string, TValue>(
                property.Key,
                property.Value.Deserialize<TValue>()));
            return true;
        }
    }
}
