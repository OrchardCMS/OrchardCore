using OrchardCore.Recipes.Schema;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdClientSettingsRecipeStep : RecipeImportStep<OpenIdClientSettingsRecipeStep.OpenIdClientSettingsStepModel>
{
    private readonly IOpenIdClientService _clientService;

    public OpenIdClientSettingsRecipeStep(IOpenIdClientService clientService)
    {
        _clientService = clientService;
    }

    public override string Name => "OpenIdClientSettings";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("OpenID Connect Client Settings")
            .Description("Imports OpenID Connect Client settings.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("DisplayName", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("Authority", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Format("uri")),
                ("ClientId", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("ClientSecret", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("CallbackPath", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("SignedOutRedirectUri", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("SignedOutCallbackPath", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("Scopes", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("Space or comma-separated scopes.")),
                ("ResponseType", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("ResponseMode", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("StoreExternalTokens", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("Parameters", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(OpenIdClientSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _clientService.LoadSettingsAsync();

        settings.Scopes = model.Scopes.Split(' ', ',');
        settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
        settings.CallbackPath = model.CallbackPath;
        settings.ClientId = model.ClientId;
        settings.ClientSecret = model.ClientSecret;
        settings.DisplayName = model.DisplayName;
        settings.ResponseMode = model.ResponseMode;
        settings.ResponseType = model.ResponseType;
        settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
        settings.SignedOutRedirectUri = model.SignedOutRedirectUri;
        settings.StoreExternalTokens = model.StoreExternalTokens;
        settings.Parameters = model.Parameters;

        await _clientService.UpdateSettingsAsync(settings);
    }

    public sealed class OpenIdClientSettingsStepModel
    {
        public string DisplayName { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public string Scopes { get; set; }
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public bool StoreExternalTokens { get; set; }
        public ParameterSetting[] Parameters { get; set; }
    }
}
