using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexRebuildRecipeStep : RecipeImportStep<AzureAISearchIndexRebuildRecipeStep.AzureAIRebuildStepModel>
{
    public override string Name => AzureAISearchIndexRebuildDeploymentSource.Name;

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Azure AI Search Index Rebuild")
            .Description("Rebuilds Azure AI Search indexes.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("IncludeAll", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("Indices", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder().TypeString())))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(AzureAIRebuildStepModel model, RecipeExecutionContext context)
    {
        if (!model.IncludeAll && (model.Indices == null || model.Indices.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(AzureAISearchIndexRebuildDeploymentSource.Name, async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
            var indexManager = scope.ServiceProvider.GetKeyedService<IIndexManager>(AzureAISearchConstants.ProviderName);

            var indexProfiles = model.IncludeAll
                ? await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName)
                : (await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName))
                    .Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var indexProfile in indexProfiles)
            {
                await indexProfileManager.ResetAsync(indexProfile);
                await indexProfileManager.UpdateAsync(indexProfile);
                await indexManager.RebuildAsync(indexProfile);
                await indexProfileManager.SynchronizeAsync(indexProfile);
            }
        });
    }

    public sealed class AzureAIRebuildStepModel
    {
        public bool IncludeAll { get; set; }
        public string[] Indices { get; set; } = [];
    }
}
