using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class RebuildIndexStep : NamedRecipeStepHandler
{
    public const string Key = "RebuildIndex";

    public RebuildIndexStep()
        : base(Key)
    {
    }

    protected override Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexProfileDeploymentStep>();

        if (model == null)
        {
            return Task.CompletedTask;
        }

        if (!model.IncludeAll && (model.IndexNames == null || model.IndexNames.Length == 0))
        {
            return Task.CompletedTask;
        }

        ShellScope.ExecuteInBackgroundAfterRequestAsync(async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetService<IIndexProfileManager>();

            var indexes = model.IncludeAll
                ? await indexProfileManager.GetAllAsync()
                : (await indexProfileManager.GetAllAsync()).Where(x => model.IndexNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            Dictionary<string, IIndexManager> indexManagers = new();

            foreach (var index in indexes)
            {
                if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
                {
                    indexManager = scope.ServiceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                    indexManagers[index.ProviderName] = indexManager;
                }

                if (indexManager is null)
                {
                    continue;
                }

                await indexProfileManager.ResetAsync(index);
                await indexProfileManager.UpdateAsync(index);
                if (await indexManager.RebuildAsync(index))
                {
                    await indexProfileManager.SynchronizeAsync(index);
                }
            }
        }, "rebuild-indexes");

        return Task.CompletedTask;
    }
}
