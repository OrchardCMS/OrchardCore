using OrchardCore.Recipes.Schema;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdValidationSettingsRecipeStep : RecipeImportStep<OpenIdValidationSettingsRecipeStep.OpenIdValidationSettingsStepModel>
{
    private readonly IOpenIdValidationService _validationService;

    public OpenIdValidationSettingsRecipeStep(IOpenIdValidationService validationService)
    {
        _validationService = validationService;
    }

    public override string Name => nameof(OpenIdValidationSettings);

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("OpenID Connect Validation Settings")
            .Description("Imports OpenID Connect Token Validation settings.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Tenant", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("MetadataAddress", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Format("uri")),
                ("Authority", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Format("uri")),
                ("Audience", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("DisableTokenTypeValidation", new RecipeStepSchemaBuilder()
                    .TypeBoolean()))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(OpenIdValidationSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _validationService.LoadSettingsAsync();

        settings.Tenant = model.Tenant;
        settings.MetadataAddress = !string.IsNullOrEmpty(model.MetadataAddress) ? new Uri(model.MetadataAddress, UriKind.Absolute) : null;
        settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
        settings.Audience = model.Audience;
        settings.DisableTokenTypeValidation = model.DisableTokenTypeValidation;

        await _validationService.UpdateSettingsAsync(settings);
    }

    public sealed class OpenIdValidationSettingsStepModel
    {
        public string Tenant { get; set; }
        public string MetadataAddress { get; set; }
        public string Authority { get; set; }
        public string Audience { get; set; }
        public bool DisableTokenTypeValidation { get; set; }
    }
}
