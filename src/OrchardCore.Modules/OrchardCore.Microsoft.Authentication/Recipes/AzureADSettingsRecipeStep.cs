using OrchardCore.Recipes.Schema;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Microsoft.Authentication.Recipes;

public sealed class AzureADSettingsRecipeStep : RecipeImportStep<AzureADSettingsRecipeStep.AzureADSettingsStepModel>
{
    private readonly IAzureADService _azureADService;

    public AzureADSettingsRecipeStep(IAzureADService azureADService)
    {
        _azureADService = azureADService;
    }

    public override string Name => nameof(AzureADSettings);

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Microsoft Entra ID Settings")
            .Description("Imports Microsoft Entra ID (Azure AD) authentication settings.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("AppId", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The application (client) ID.")),
                ("TenantId", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The directory (tenant) ID.")),
                ("DisplayName", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The display name for the authentication provider.")),
                ("CallbackPath", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The callback path for authentication.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(AzureADSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _azureADService.LoadSettingsAsync();

        settings.AppId = model.AppId;
        settings.TenantId = model.TenantId;
        settings.DisplayName = model.DisplayName;
        settings.CallbackPath = model.CallbackPath;

        await _azureADService.UpdateSettingsAsync(settings);
    }

    public sealed class AzureADSettingsStepModel
    {
        public string DisplayName { get; set; }
        public string AppId { get; set; }
        public string TenantId { get; set; }
        public string CallbackPath { get; set; }
    }
}
