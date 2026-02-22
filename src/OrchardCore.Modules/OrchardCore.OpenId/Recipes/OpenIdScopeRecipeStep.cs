using OrchardCore.Recipes.Schema;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdScopeRecipeStep : RecipeImportStep<OpenIdScopeRecipeStep.OpenIdScopeStepModel>
{
    private readonly IOpenIdScopeManager _scopeManager;

    public OpenIdScopeRecipeStep(IOpenIdScopeManager scopeManager)
    {
        _scopeManager = scopeManager;
    }

    public override string Name => "OpenIdScope";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("OpenID Connect Scope")
            .Description("Creates or updates an OpenID Connect scope.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ScopeName", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The scope name.")),
                ("DisplayName", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The display name for the scope.")),
                ("Description", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The description for the scope.")),
                ("Resources", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("Space-separated resource identifiers.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(OpenIdScopeStepModel model, RecipeExecutionContext context)
    {
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

    public sealed class OpenIdScopeStepModel
    {
        public string ScopeName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Resources { get; set; }
    }
}
