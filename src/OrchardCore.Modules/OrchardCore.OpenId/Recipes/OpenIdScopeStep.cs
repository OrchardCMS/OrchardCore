using System.Text.Json.Nodes;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class OpenIdScopeStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    private readonly IOpenIdScopeManager _scopeManager;

    /// <summary>
    /// This recipe step adds an OpenID Connect scope.
    /// </summary>
    public OpenIdScopeStep(IOpenIdScopeManager scopeManager)
        : base("OpenIdScope")
    {
        _scopeManager = scopeManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<OpenIdScopeStepModel>();
        var scope = await _scopeManager.FindByNameAsync(model.ScopeName);
        var descriptor = new OpenIdScopeDescriptor();
        var isNew = true;

        if (scope != null)
        {
            isNew = false;
            await _scopeManager.PopulateAsync(scope, descriptor);
        }

        descriptor.Description = model.Description;
        descriptor.Name = model.ScopeName;
        descriptor.DisplayName = model.DisplayName;

        if (!string.IsNullOrEmpty(model.Resources))
        {
            descriptor.Resources.Clear();
            descriptor.Resources.UnionWith(
                model.Resources.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        if (isNew)
        {
            await _scopeManager.CreateAsync(descriptor);
        }
        else
        {
            await _scopeManager.UpdateAsync(scope, descriptor);
        }
    }
}
