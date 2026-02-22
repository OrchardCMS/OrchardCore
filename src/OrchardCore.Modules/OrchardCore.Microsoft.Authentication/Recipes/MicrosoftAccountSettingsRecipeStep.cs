using OrchardCore.Recipes.Schema;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Microsoft.Authentication.Recipes;

public sealed class MicrosoftAccountSettingsRecipeStep : RecipeImportStep<MicrosoftAccountSettingsRecipeStep.MicrosoftAccountSettingsStepModel>
{
    private readonly IMicrosoftAccountService _microsoftAccountService;

    public MicrosoftAccountSettingsRecipeStep(IMicrosoftAccountService microsoftAccountService)
    {
        _microsoftAccountService = microsoftAccountService;
    }

    public override string Name => nameof(MicrosoftAccountSettings);

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Microsoft Account Settings")
            .Description("Imports Microsoft Account authentication settings.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("AppId", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The application (client) ID.")),
                ("AppSecret", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The application secret.")),
                ("CallbackPath", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The callback path for authentication.")))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(MicrosoftAccountSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _microsoftAccountService.LoadSettingsAsync();

        settings.AppId = model.AppId;
        settings.AppSecret = model.AppSecret;
        settings.CallbackPath = model.CallbackPath;

        await _microsoftAccountService.UpdateSettingsAsync(settings);
    }

    public sealed class MicrosoftAccountSettingsStepModel
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string CallbackPath { get; set; }
    }
}
