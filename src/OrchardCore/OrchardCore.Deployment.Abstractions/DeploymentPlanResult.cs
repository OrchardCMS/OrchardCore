using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment
{
    /// <summary>
    /// The state of a deployment plan built by sources.
    /// </summary>
    public class DeploymentPlanResult
    {
        public DeploymentPlanResult(IFileBuilder fileBuilder, RecipeDescriptor recipeDescriptor)
        {
            FileBuilder = fileBuilder;

            Recipe = new JsonObject
            {
                ["name"] = recipeDescriptor.Name ?? "",
                ["displayName"] = recipeDescriptor.DisplayName ?? "",
                ["description"] = recipeDescriptor.Description ?? "",
                ["author"] = recipeDescriptor.Author ?? "",
                ["website"] = recipeDescriptor.WebSite ?? "",
                ["version"] = recipeDescriptor.Version ?? "",
                ["issetuprecipe"] = recipeDescriptor.IsSetupRecipe,
                ["categories"] = new JsonArray(recipeDescriptor.Categories ?? Array.Empty<string>()),
                ["tags"] = new JsonArray(recipeDescriptor.Tags ?? Array.Empty<string>()),
            };
        }

        public JsonObject Recipe { get; }
        public IList<JsonObject> Steps { get; } = new List<JsonObject>();
        public IFileBuilder FileBuilder { get; }
        public async Task FinalizeAsync()
        {
            Recipe["steps"] = new JArray(Steps);

            // Add the recipe steps as its own file content
            await FileBuilder.SetFileAsync("Recipe.json", Encoding.UTF8.GetBytes(Recipe.ToString(Formatting.Indented)));
        }

        /// <summary>
        /// Adds an entry to <see cref="Steps"/>.
        /// </summary>
        /// <param name="name">The <c>name</c> property of the step object.</param>
        /// <param name="properties">Other properties of the step object.</param>
        public void AddStep(string name, IEnumerable<KeyValuePair<string, JsonNode>> properties)
        {
            var step = new JsonObject { ["name"] = name };
            foreach (var property in properties) step.Add(property);
            Steps.Add(step);
        }

        /// <summary>
        /// Adds an entry to <see cref="Steps"/> with a name and one or no additional property.
        /// </summary>
        /// <param name="name">The <c>name</c> property of the step object.</param>
        /// <param name="key">The key of the additional property, or <see langword="null"/>.</param>
        /// <param name="value">The value of the additional property.</param>
        public void AddSimpleStep(string name, string key, JsonNode value) =>
            AddStep(name, key == null ? Array.Empty<KeyValuePair<string, JsonNode>>() : new [] { new KeyValuePair<string, JsonNode>(key, value) });

        public void AddSimpleStepAndSerializeValue<T>(string name, string key, T value) =>
            AddStep(name, new [] { new KeyValuePair<string, JsonNode>(key, JsonSerializer.SerializeToNode(value)) });
    }
}
