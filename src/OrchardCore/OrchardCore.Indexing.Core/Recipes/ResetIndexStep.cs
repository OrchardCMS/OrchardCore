using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class ResetIndexStep : NamedRecipeStepHandler
{
    public const string Key = "ResetIndex";

    private readonly IServiceProvider _services;

    public ResetIndexStep(IServiceProvider services)
        : base(Key)
    {
        _services = services;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexProfileDeploymentStep>();

        if (model == null)
        {
            return;
        }

        if (!model.IncludeAll && (model.IndexNames == null || model.IndexNames.Length == 0))
        {
            return;
        }

        var profiles = _services.GetRequiredService<IIndexProfileManager>();
        var operations = _services.GetRequiredService<Operations.IndexOperationRunner>();
        var indexes = await profiles.GetAllAsync();
        foreach (var index in indexes.Where(index => model.IncludeAll || model.IndexNames.Contains(index.Name, StringComparer.OrdinalIgnoreCase)))
        {
            await operations.QueueAsync(index.Id, IndexLifecycleAction.Reset);
        }
    }
}
