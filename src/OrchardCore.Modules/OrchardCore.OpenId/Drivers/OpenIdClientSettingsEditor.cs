using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Secrets;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Maps the <see cref="OpenIdClientSettings"/> to their editor, shared by the client settings of the site and the
/// additional client registrations.
/// </summary>
internal static class OpenIdClientSettingsEditor
{
    private static readonly char[] _scopeSeparators = [' ', ','];

    public static void Populate(OpenIdClientSettingsViewModel model, OpenIdClientSettings settings, IServiceProvider serviceProvider)
    {
        model.DisplayName = settings.DisplayName;
        model.Scopes = settings.Scopes != null ? string.Join(" ", settings.Scopes) : null;
        model.Authority = settings.Authority?.AbsoluteUri;
        model.CallbackPath = settings.CallbackPath;
        model.ClientId = settings.ClientId;
        model.ClientSecret = SecretInputViewModel.Create(settings.ClientSecret, settings.ClientSecretSecretName);
        model.SigningKeySecretName = settings.SigningKeySecretName;
        model.SecretsEnabled = serviceProvider.GetService<ISecretManager>() is not null;
        model.SignedOutCallbackPath = settings.SignedOutCallbackPath;
        model.SignedOutRedirectUri = settings.SignedOutRedirectUri;
        model.ResponseMode = settings.ResponseMode;
        model.StoreExternalTokens = settings.StoreExternalTokens;

        switch (settings.ResponseType)
        {
            case OpenIdConstants.Client.ResponseTypes.Code:
                model.UseCodeFlow = true;
                break;
            case OpenIdConstants.Client.ResponseTypes.CodeIdToken:
                model.UseCodeIdTokenFlow = true;
                break;
            case OpenIdConstants.Client.ResponseTypes.CodeIdTokenToken:
                model.UseCodeIdTokenTokenFlow = true;
                break;
            case OpenIdConstants.Client.ResponseTypes.CodeToken:
                model.UseCodeTokenFlow = true;
                break;
            case OpenIdConstants.Client.ResponseTypes.IdToken:
                model.UseIdTokenFlow = true;
                break;
            case OpenIdConstants.Client.ResponseTypes.IdTokenToken:
                model.UseIdTokenTokenFlow = true;
                break;
        }

        model.Parameters = JConvert.SerializeObject(settings.Parameters, JOptions.CamelCase);
    }

    public static async Task UpdateAsync(
        OpenIdClientSettingsViewModel model,
        OpenIdClientSettings settings,
        ModelStateDictionary modelState,
        string prefix,
        IServiceProvider serviceProvider,
        IDataProtectionProvider dataProtectionProvider,
        IStringLocalizer S)
    {
        model.Scopes ??= string.Empty;

        settings.DisplayName = model.DisplayName;
        settings.Scopes = model.Scopes.Split(_scopeSeparators, StringSplitOptions.RemoveEmptyEntries);
        settings.Authority = Uri.TryCreate(model.Authority, UriKind.Absolute, out var authority) ? authority : null;
        settings.CallbackPath = model.CallbackPath;
        settings.ClientId = model.ClientId;
        settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
        settings.SignedOutRedirectUri = model.SignedOutRedirectUri;
        settings.ResponseMode = model.ResponseMode;
        settings.StoreExternalTokens = model.StoreExternalTokens;

        var useClientSecret = true;

        if (model.UseCodeFlow)
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.Code;
        }
        else if (model.UseCodeIdTokenFlow)
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.CodeIdToken;
        }
        else if (model.UseCodeIdTokenTokenFlow)
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.CodeIdTokenToken;
        }
        else if (model.UseCodeTokenFlow)
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.CodeToken;
        }
        else if (model.UseIdTokenFlow)
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.IdToken;
            useClientSecret = false;
        }
        else if (model.UseIdTokenTokenFlow)
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.IdTokenToken;
            useClientSecret = false;
        }
        else
        {
            settings.ResponseType = OpenIdConstants.Client.ResponseTypes.None;
            useClientSecret = false;
        }

        try
        {
            settings.Parameters = string.IsNullOrWhiteSpace(model.Parameters)
                ? []
                : JConvert.DeserializeObject<ParameterSetting[]>(model.Parameters);
        }
        catch (JsonException)
        {
            modelState.AddModelError(prefix, S["The parameters are written in an incorrect format."]);
        }

        if (useClientSecret)
        {
            var clientSecret = await model.ClientSecret.UpdateAsync(new SecretInputUpdateContext(
                serviceProvider,
                dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration)),
                modelState,
                $"{prefix}.{nameof(model.ClientSecret)}")
            {
                ProtectedValue = settings.ClientSecret,
                SecretName = settings.ClientSecretSecretName,
            });

            if (clientSecret.Succeeded)
            {
                settings.ClientSecret = clientSecret.ProtectedValue;
                settings.ClientSecretSecretName = clientSecret.SecretName;
            }

            // The signing key is only offered while the Secrets feature is enabled, so it's kept otherwise.
            if (serviceProvider.GetService<ISecretManager>() is not null)
            {
                settings.SigningKeySecretName = string.IsNullOrWhiteSpace(model.SigningKeySecretName) ? null : model.SigningKeySecretName;
            }
        }
    }

    /// <summary>
    /// Adds the errors of a validation to the model state, under the field of the editor they belong to.
    /// </summary>
    public static void AddErrors(IEnumerable<ValidationResult> results, ModelStateDictionary modelState, string prefix)
    {
        foreach (var result in results)
        {
            if (result != ValidationResult.Success)
            {
                var member = result.MemberNames.FirstOrDefault();
                var key = string.IsNullOrEmpty(member) ? prefix
                    : string.IsNullOrEmpty(prefix) ? member
                    : $"{prefix}.{member}";

                // A field the editor already rejected, e.g. a required field left empty, keeps its first error.
                if (modelState.TryGetValue(key, out var entry) && entry.Errors.Count > 0)
                {
                    continue;
                }

                modelState.AddModelError(key, result.ErrorMessage);
            }
        }
    }
}
