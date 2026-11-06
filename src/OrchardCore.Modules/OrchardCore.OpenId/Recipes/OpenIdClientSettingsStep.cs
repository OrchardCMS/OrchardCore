using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

/// <summary>
/// This recipe step sets general OpenID Connect Client settings.
/// </summary>
public sealed class OpenIdClientSettingsStep : NamedRecipeStepHandler
{
    private readonly IOpenIdClientService _clientService;
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public OpenIdClientSettingsStep(
        IOpenIdClientService clientService,
        IDataProtectionProvider dataProtectionProvider)
        : base("OpenIdClientSettings")
    {
        _clientService = clientService;
        _dataProtectionProvider = dataProtectionProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<OpenIdClientSettingsStepModel>();
        var settings = await _clientService.LoadSettingsAsync();

        model.ApplyTo(settings, _dataProtectionProvider);

        await _clientService.UpdateSettingsAsync(settings);
    }
}

public class OpenIdClientSettingsStepModel
{
    private static readonly char[] _scopeSeparators = [' ', ','];

    public string DisplayName { get; set; }

    [Url]
    public string Authority { get; set; }

    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret, in plain text. It's protected with Data Protection when it's stored.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the client secret.
    /// </summary>
    public string ClientSecretSecretName { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the key used to sign client assertions.
    /// </summary>
    public string SigningKeySecretName { get; set; }

    public string CallbackPath { get; set; }
    public string SignedOutRedirectUri { get; set; }
    public string SignedOutCallbackPath { get; set; }
    public string Scopes { get; set; }
    public string ResponseType { get; set; }
    public string ResponseMode { get; set; }
    public bool StoreExternalTokens { get; set; }
    public ParameterSetting[] Parameters { get; set; }

    internal void ApplyTo(OpenIdClientSettings settings, IDataProtectionProvider dataProtectionProvider)
    {
        settings.Scopes = Scopes?.Split(_scopeSeparators, StringSplitOptions.RemoveEmptyEntries) ?? [];
        settings.Authority = !string.IsNullOrEmpty(Authority) ? new Uri(Authority, UriKind.Absolute) : null;
        settings.CallbackPath = CallbackPath;
        settings.ClientId = ClientId;
        settings.DisplayName = DisplayName;
        settings.ResponseMode = ResponseMode;
        settings.ResponseType = ResponseType;
        settings.SignedOutCallbackPath = SignedOutCallbackPath;
        settings.SignedOutRedirectUri = SignedOutRedirectUri;
        settings.StoreExternalTokens = StoreExternalTokens;
        settings.Parameters = Parameters ?? [];
        settings.ClientSecretSecretName = ClientSecretSecretName;
        settings.SigningKeySecretName = SigningKeySecretName;

        // The client secret is read with the protector the settings editors use, so it's stored protected.
        // The current secret is kept when none is provided, e.g. by a recipe created from a deployment plan,
        // which never exports the secrets.
        if (!string.IsNullOrEmpty(ClientSecret))
        {
            settings.ClientSecret = dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration)).Protect(ClientSecret);
        }
    }
}
