using System.Text.Json.Nodes;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdScopeStep : NamedRecipeStepHandler
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
        await OpenIdScopeEditor.SaveAsync(_scopeManager, scope, model.ScopeName, model.DisplayName, model.Description,
            string.IsNullOrEmpty(model.Resources) ? null : model.Resources.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
