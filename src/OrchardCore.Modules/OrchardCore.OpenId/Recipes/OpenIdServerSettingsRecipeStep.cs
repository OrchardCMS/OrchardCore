using OrchardCore.Recipes.Schema;
using Microsoft.AspNetCore.Http;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using System.Security.Cryptography.X509Certificates;

namespace OrchardCore.OpenId.Recipes;

public sealed class OpenIdServerSettingsRecipeStep : RecipeImportStep<OpenIdServerSettingsRecipeStep.OpenIdServerSettingsStepModel>
{
    private readonly IOpenIdServerService _serverService;

    public OpenIdServerSettingsRecipeStep(IOpenIdServerService serverService)
    {
        _serverService = serverService;
    }

    public override string Name => nameof(OpenIdServerSettings);

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("OpenID Connect Server Settings")
            .Description("Imports OpenID Connect Server settings.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("AccessTokenFormat", new RecipeStepSchemaBuilder()
                    .TypeInteger()),
                ("Authority", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Format("uri")),
                ("EncryptionCertificateStoreLocation", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("EncryptionCertificateStoreName", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("EncryptionCertificateThumbprint", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("SigningCertificateStoreLocation", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("SigningCertificateStoreName", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("SigningCertificateThumbprint", new RecipeStepSchemaBuilder()
                    .TypeString()),
                ("EnableAuthorizationEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("EnableLogoutEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("EnableTokenEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("EnableUserInfoEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("EnableIntrospectionEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("EnablePushedAuthorizationEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("EnableRevocationEndpoint", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowAuthorizationCodeFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowClientCredentialsFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowHybridFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowImplicitFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowPasswordFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("AllowRefreshTokenFlow", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("DisableAccessTokenEncryption", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("DisableRollingRefreshTokens", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("UseReferenceAccessTokens", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("RequireProofKeyForCodeExchange", new RecipeStepSchemaBuilder()
                    .TypeBoolean()),
                ("RequirePushedAuthorizationRequests", new RecipeStepSchemaBuilder()
                    .TypeBoolean()))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(OpenIdServerSettingsStepModel model, RecipeExecutionContext context)
    {
        var settings = await _serverService.LoadSettingsAsync();

        settings.AccessTokenFormat = model.AccessTokenFormat;
        settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;

        settings.EncryptionCertificateStoreLocation = model.EncryptionCertificateStoreLocation;
        settings.EncryptionCertificateStoreName = model.EncryptionCertificateStoreName;
        settings.EncryptionCertificateThumbprint = model.EncryptionCertificateThumbprint;

        settings.SigningCertificateStoreLocation = model.SigningCertificateStoreLocation;
        settings.SigningCertificateStoreName = model.SigningCertificateStoreName;
        settings.SigningCertificateThumbprint = model.SigningCertificateThumbprint;

        settings.AuthorizationEndpointPath = model.EnableAuthorizationEndpoint ?
            new PathString("/connect/authorize") : PathString.Empty;
        settings.LogoutEndpointPath = model.EnableLogoutEndpoint ?
            new PathString("/connect/logout") : PathString.Empty;
        settings.TokenEndpointPath = model.EnableTokenEndpoint ?
            new PathString("/connect/token") : PathString.Empty;
        settings.UserinfoEndpointPath = model.EnableUserInfoEndpoint ?
            new PathString("/connect/userinfo") : PathString.Empty;
        settings.IntrospectionEndpointPath = model.EnableIntrospectionEndpoint ?
            new PathString("/connect/introspect") : PathString.Empty;
        settings.PushedAuthorizationEndpointPath = model.EnablePushedAuthorizationEndpoint ?
            new PathString("/connect/par") : PathString.Empty;
        settings.RevocationEndpointPath = model.EnableRevocationEndpoint ?
            new PathString("/connect/revoke") : PathString.Empty;

        settings.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
        settings.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
        settings.AllowHybridFlow = model.AllowHybridFlow;
        settings.AllowImplicitFlow = model.AllowImplicitFlow;
        settings.AllowPasswordFlow = model.AllowPasswordFlow;
        settings.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;

        settings.DisableAccessTokenEncryption = model.DisableAccessTokenEncryption;
        settings.DisableRollingRefreshTokens = model.DisableRollingRefreshTokens;
        settings.UseReferenceAccessTokens = model.UseReferenceAccessTokens;
        settings.RequireProofKeyForCodeExchange = model.RequireProofKeyForCodeExchange;
        settings.RequirePushedAuthorizationRequests = model.RequirePushedAuthorizationRequests;

        await _serverService.UpdateSettingsAsync(settings);
    }

    public sealed class OpenIdServerSettingsStepModel
    {
        public OpenIdServerSettings.TokenFormat AccessTokenFormat { get; set; }
        public string Authority { get; set; }
        public StoreLocation? EncryptionCertificateStoreLocation { get; set; }
        public StoreName? EncryptionCertificateStoreName { get; set; }
        public string EncryptionCertificateThumbprint { get; set; }
        public StoreLocation? SigningCertificateStoreLocation { get; set; }
        public StoreName? SigningCertificateStoreName { get; set; }
        public string SigningCertificateThumbprint { get; set; }
        public bool EnableAuthorizationEndpoint { get; set; }
        public bool EnableLogoutEndpoint { get; set; }
        public bool EnableTokenEndpoint { get; set; }
        public bool EnableUserInfoEndpoint { get; set; }
        public bool EnableIntrospectionEndpoint { get; set; }
        public bool EnablePushedAuthorizationEndpoint { get; set; }
        public bool EnableRevocationEndpoint { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool AllowPasswordFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool DisableAccessTokenEncryption { get; set; }
        public bool DisableRollingRefreshTokens { get; set; }
        public bool UseReferenceAccessTokens { get; set; }
        public bool RequireProofKeyForCodeExchange { get; set; }
        public bool RequirePushedAuthorizationRequests { get; set; }
    }
}
