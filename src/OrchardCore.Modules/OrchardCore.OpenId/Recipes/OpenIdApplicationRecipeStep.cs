using OrchardCore.Recipes.Schema;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdApplicationRecipeStep : RecipeImportStep<OpenIdApplicationRecipeStep.OpenIdApplicationStepModel>
{
    private readonly IOpenIdApplicationManager _applicationManager;

    public OpenIdApplicationRecipeStep(IOpenIdApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    public override string Name => "OpenIdApplication";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("OpenID Connect Application")
            .Description("Creates or updates an OpenID Connect application.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("ClientId", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The client identifier.")),
                ("ClientSecret", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The client secret.")),
                ("DisplayName", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The display name of the application.")),
                ("Type", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The application type.")),
                ("ConsentType", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("The consent type.")),
                ("AllowAuthorizationCodeFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowClientCredentialsFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowHybridFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowImplicitFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowIntrospectionEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowLogoutEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowPasswordFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowRefreshTokenFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowRevocationEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("RequireProofKeyForCodeExchange", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("RequirePushedAuthorizationRequests", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("RedirectUris", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("Space-separated redirect URIs.")),
                ("PostLogoutRedirectUris", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Description("Space-separated post-logout redirect URIs.")),
                ("RoleEntries", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(("Name", new RecipeStepSchemaBuilder().TypeString()))
                        .AdditionalProperties(true))),
                ("ScopeEntries", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(("Name", new RecipeStepSchemaBuilder().TypeString()))
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(OpenIdApplicationStepModel model, RecipeExecutionContext context)
    {
        var app = await _applicationManager.FindByClientIdAsync(model.ClientId);

        var settings = new OpenIdApplicationSettings()
        {
            AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow,
            AllowClientCredentialsFlow = model.AllowClientCredentialsFlow,
            AllowHybridFlow = model.AllowHybridFlow,
            AllowImplicitFlow = model.AllowImplicitFlow,
            AllowIntrospectionEndpoint = model.AllowIntrospectionEndpoint,
            AllowLogoutEndpoint = model.AllowLogoutEndpoint,
            AllowPasswordFlow = model.AllowPasswordFlow,
            AllowRefreshTokenFlow = model.AllowRefreshTokenFlow,
            AllowRevocationEndpoint = model.AllowRevocationEndpoint,
            ClientId = model.ClientId,
            ClientSecret = model.ClientSecret,
            ConsentType = model.ConsentType,
            DisplayName = model.DisplayName,
            PostLogoutRedirectUris = model.PostLogoutRedirectUris,
            RedirectUris = model.RedirectUris,
            Roles = model.RoleEntries.Select(x => x.Name).ToArray(),
            Scopes = model.ScopeEntries.Select(x => x.Name).ToArray(),
            Type = model.Type,
            RequireProofKeyForCodeExchange = model.RequireProofKeyForCodeExchange,
            RequirePushedAuthorizationRequests = model.RequirePushedAuthorizationRequests,
        };

        await _applicationManager.UpdateDescriptorFromSettings(settings, app);
    }

    public sealed class OpenIdApplicationStepModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string ConsentType { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowIntrospectionEndpoint { get; set; }
        public bool AllowLogoutEndpoint { get; set; }
        public bool AllowPasswordFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowRevocationEndpoint { get; set; }
        public bool RequireProofKeyForCodeExchange { get; set; }
        public bool RequirePushedAuthorizationRequests { get; set; }
        public string RedirectUris { get; set; }
        public string PostLogoutRedirectUris { get; set; }
        public RoleEntry[] RoleEntries { get; set; } = [];
        public ScopeEntry[] ScopeEntries { get; set; } = [];
    }

    public sealed class RoleEntry
    {
        public string Name { get; set; }
    }

    public sealed class ScopeEntry
    {
        public string Name { get; set; }
    }
}
