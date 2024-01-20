using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;

namespace OrchardCore.ContentTypes
{
    /// <summary>
    /// This handler provides backward compatibility with ContentIndexSettings that have been migrated to LuceneContentIndexSettings.
    /// </summary>
    public class LuceneRecipeEventHandler : IRecipeEventHandler
    {
        public RecipeExecutionContext Context { get; private set; }

        public Task RecipeStepExecutedAsync(RecipeExecutionContext context) => Task.CompletedTask;

        public Task ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

        public Task RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

        public Task RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

        public Task RecipeStepExecutingAsync(RecipeExecutionContext context)
        {
            if (context.Name == "ReplaceContentDefinition" || context.Name == "ContentDefinition")
            {
                var step = context.Step.ToObject<ContentDefinitionStepModel>();

                foreach (var contentType in step.ContentTypes)
                {
                    foreach (var partDefinition in contentType.ContentTypePartDefinitionRecords)
                    {
                        if (partDefinition.Settings != null)
                        {
                            if (partDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingPartSettings) &&
                                !partDefinition.Settings.ContainsKey("LuceneContentIndexSettings"))
                            {
                                partDefinition.Settings.Add(new JProperty("LuceneContentIndexSettings", existingPartSettings));
                            }

                            partDefinition.Settings.Remove("ContentIndexSettings");
                        }
                    }
                }

                foreach (var partDefinition in step.ContentParts)
                {
                    if (partDefinition.Settings != null)
                    {
                        if (partDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingPartSettings) &&
                            !partDefinition.Settings.ContainsKey("LuceneContentIndexSettings"))
                        {
                            partDefinition.Settings.Add(new JProperty("LuceneContentIndexSettings", existingPartSettings));
                        }

                        partDefinition.Settings.Remove("ContentIndexSettings");

                        foreach (var fieldDefinition in partDefinition.ContentPartFieldDefinitionRecords)
                        {
                            if (fieldDefinition.Settings != null)
                            {
                                if (fieldDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingFieldSettings) &&
                                    !fieldDefinition.Settings.ContainsKey("LuceneContentIndexSettings"))
                                {
                                    fieldDefinition.Settings.Add(new JProperty("LuceneContentIndexSettings", existingFieldSettings));
                                }

                                fieldDefinition.Settings.Remove("ContentIndexSettings");
                            }
                        }
                    }
                }

                context.Step = JObject.FromObject(step);
            }

            return Task.CompletedTask;
        }

        private class ContentDefinitionStepModel
        {
            public string Name { get; set; }
            public ContentTypeDefinitionRecord[] ContentTypes { get; set; } = Array.Empty<ContentTypeDefinitionRecord>();
            public ContentPartDefinitionRecord[] ContentParts { get; set; } = Array.Empty<ContentPartDefinitionRecord>();
        }
    }
}
