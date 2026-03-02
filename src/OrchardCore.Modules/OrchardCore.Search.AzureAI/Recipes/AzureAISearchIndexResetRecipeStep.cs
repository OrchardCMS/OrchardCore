using OrchardCore.Recipes.Schema;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexResetRecipeStep : RecipeImportStep<AzureAISearchIndexResetRecipeStep.AzureAIResetStepModel>
{
    public override string Name => AzureAISearchIndexResetDeploymentSource.Name;

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Azure AI Search Index Reset")
            .Description("Resets Azure AI Search indexes.")
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

    protected override async Task ImportAsync(AzureAIResetStepModel model, RecipeExecutionContext context)
    {
        if (!model.IncludeAll && (model.Indices == null || model.Indices.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(AzureAISearchIndexRebuildDeploymentSource.Name, async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();

            var indexes = model.IncludeAll
                ? await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName)
                : (await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName))
                    .Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexProfileManager.ResetAsync(index);
                await indexProfileManager.UpdateAsync(index);
                await indexProfileManager.SynchronizeAsync(index);
            }
        });
    }

    public sealed class AzureAIResetStepModel
    {
        public bool IncludeAll { get; set; }
        public string[] Indices { get; set; } = [];
    }
}
