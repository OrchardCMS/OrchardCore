using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;

namespace OrchardCore.OpenId;

public class OpenIdApplicationSettings
{
    public string ClientId { get; set; }
    public string DisplayName { get; set; }
    public string RedirectUris { get; set; }
    public string PostLogoutRedirectUris { get; set; }
    public string ApplicationType { get; set; }
    public string Type { get; set; }
    public string ConsentType { get; set; }
    public string ClientSecret { get; set; }
    public string[] Roles { get; set; }
    public string[] Scopes { get; set; }
    public bool AllowPasswordFlow { get; set; }
    public bool AllowClientCredentialsFlow { get; set; }
    public bool AllowAuthorizationCodeFlow { get; set; }
    public bool AllowDeviceAuthorizationFlow { get; set; }
    public bool AllowRefreshTokenFlow { get; set; }
    public bool AllowHybridFlow { get; set; }
    public bool AllowImplicitFlow { get; set; }
    public bool AllowLogoutEndpoint { get; set; }
    public bool AllowIntrospectionEndpoint { get; set; }
    public bool AllowRevocationEndpoint { get; set; }
    public bool RequireProofKeyForCodeExchange { get; set; }
    public bool RequirePushedAuthorizationRequests { get; set; }
}

internal static class OpenIdApplicationExtensions
{
    internal static readonly string[] s_separator = [" ", ","];

    internal static IEnumerable<ValidationResult> ValidateClientSettings(string clientType, string applicationType,
        string clientSecret, IStringLocalizer S, bool isNew, bool wasPublic = false)
    {
        var isPublic = string.Equals(clientType, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(clientSecret) && isPublic)
        {
            yield return new ValidationResult(S["No client secret can be set for public applications."], [nameof(OpenIdApplicationSettings.ClientSecret)]);
        }
        else if (string.IsNullOrEmpty(clientSecret))
        {
            if (isNew && string.Equals(clientType, OpenIddictConstants.ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(S["The client secret is required for confidential applications."], [nameof(OpenIdApplicationSettings.ClientSecret)]);
            }
            else if (!isNew && wasPublic && !isPublic)
            {
                yield return new ValidationResult(S["Setting a new client secret is required."], [nameof(OpenIdApplicationSettings.ClientSecret)]);
            }
        }

        if (string.Equals(applicationType, OpenIddictConstants.ApplicationTypes.Native, StringComparison.OrdinalIgnoreCase) && !isPublic)
        {
            yield return new ValidationResult(S["Native applications must be public clients."], [nameof(OpenIdApplicationSettings.Type)]);
        }
    }

    public static async Task UpdateDescriptorFromSettings(this IOpenIdApplicationManager manager, OpenIdApplicationSettings model,
        object application = null, CancellationToken cancellationToken = default)
    {
        var descriptor = await manager.BuildDescriptorFromSettingsAsync(model, application, cancellationToken);
        if (application is null)
        {
            await manager.CreateAsync(descriptor, cancellationToken);
            return;
        }

        await manager.UpdateWithValidationRollbackAsync(application,
            () => manager.UpdateAsync(application, descriptor, cancellationToken), cancellationToken);
    }

    internal static async Task UpdateWithValidationRollbackAsync(this IOpenIdApplicationManager manager, object application,
        Func<ValueTask> update, CancellationToken cancellationToken)
    {
        var original = new OpenIdApplicationDescriptor();
        await manager.PopulateAsync(original, application, cancellationToken);
        try
        {
            await update();
        }
        catch (OpenIddictExceptions.ValidationException)
        {
            // OpenIddict populates the tracked entity before validating the descriptor.
            // Restore the previous values so rejected edits cannot contaminate readback.
            await manager.PopulateAsync(application, original, CancellationToken.None);
            throw;
        }
    }

    internal static async Task<object> FindByClientIdForUpdateAsync(this IOpenIdApplicationManager manager, string clientId, CancellationToken cancellationToken)
    {
        var application = await manager.FindByClientIdAsync(clientId, cancellationToken);
        // Natural-key lookup can return a cached object. Like the admin editor,
        // load the store's tracked instance before mutating or deleting it.
        return application is null ? null : await manager.FindByPhysicalIdAsync(
            await manager.GetPhysicalIdAsync(application, cancellationToken), cancellationToken);
    }

    internal static async Task<OpenIdApplicationDescriptor> BuildDescriptorFromSettingsAsync(this IOpenIdApplicationManager _applicationManager,
        OpenIdApplicationSettings model, object application = null, CancellationToken cancellationToken = default)
    {
        var descriptor = new OpenIdApplicationDescriptor();

        if (application != null)
        {
            await _applicationManager.PopulateAsync(descriptor, application, cancellationToken);
        }

        descriptor.ClientId = model.ClientId;
        descriptor.ConsentType = model.ConsentType;
        descriptor.DisplayName = model.DisplayName;
        descriptor.ClientType = model.Type;
        descriptor.ApplicationType = model.ApplicationType;

        if (!string.IsNullOrEmpty(model.ClientSecret))
        {
            descriptor.ClientSecret = model.ClientSecret;
        }

        if (string.Equals(descriptor.ClientType, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
        {
            descriptor.ClientSecret = null;
        }

        if (model.AllowLogoutEndpoint)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.EndSession);
        }
        else
        {
            descriptor.Permissions.Remove("ept:logout"); // Still allowed for backcompat reasons.
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.EndSession);
        }

        if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        }

        if (model.AllowClientCredentialsFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
        }

        if (model.AllowHybridFlow || model.AllowImplicitFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.Implicit);
        }

        if (model.AllowPasswordFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.Password);
        }

        if (model.AllowRefreshTokenFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        }

        if (model.AllowDeviceAuthorizationFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.DeviceCode);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.DeviceCode);
        }

        if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow || model.AllowImplicitFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.PushedAuthorization);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Authorization);
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.PushedAuthorization);
        }

        if (model.AllowAuthorizationCodeFlow || model.AllowHybridFlow ||
            model.AllowClientCredentialsFlow || model.AllowDeviceAuthorizationFlow ||
            model.AllowPasswordFlow || model.AllowRefreshTokenFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Token);
        }

        if (model.AllowDeviceAuthorizationFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization);
        }

        if (model.AllowAuthorizationCodeFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.Code);
        }

        if (model.AllowImplicitFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);

            if (string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.Token);
            }
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.Token);
        }

        if (model.AllowHybridFlow)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

            if (string.Equals(model.Type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
            }
            else
            {
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
            }
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
        }

        if (model.AllowIntrospectionEndpoint)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Introspection);
        }

        if (model.AllowRevocationEndpoint)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
        }
        else
        {
            descriptor.Permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Revocation);
        }

        if (model.RequireProofKeyForCodeExchange)
        {
            descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        }
        else
        {
            descriptor.Requirements.Remove(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
        }

        if (model.RequirePushedAuthorizationRequests)
        {
            descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.PushedAuthorizationRequests);
        }
        else
        {
            descriptor.Requirements.Remove(OpenIddictConstants.Requirements.Features.PushedAuthorizationRequests);
        }

        descriptor.Roles.Clear();

        foreach (var role in model.Roles)
        {
            descriptor.Roles.Add(role);
        }

        descriptor.Permissions.RemoveWhere(permission => permission.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope));
        foreach (var scope in model.Scopes)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
        }

        descriptor.PostLogoutRedirectUris.Clear();
        foreach (var uri in
            (from uri in model.PostLogoutRedirectUris?.Split(s_separator, StringSplitOptions.RemoveEmptyEntries) ?? []
             select new Uri(uri, UriKind.Absolute)))
        {
            descriptor.PostLogoutRedirectUris.Add(uri);
        }

        descriptor.RedirectUris.Clear();
        foreach (var uri in
           (from uri in model.RedirectUris?.Split(s_separator, StringSplitOptions.RemoveEmptyEntries) ?? []
            select new Uri(uri, UriKind.Absolute)))
        {
            descriptor.RedirectUris.Add(uri);
        }

        return descriptor;
    }
}
