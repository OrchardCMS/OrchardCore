using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;
using static OrchardCore.OpenId.Services.OpenIdSettingsField;

namespace OrchardCore.OpenId;

/// <summary>
/// Registers tenant-owned OpenID server settings management.
/// </summary>
[Feature(OpenIdConstants.Features.Server)]
public sealed class OpenIdServerManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider =>
        {
            var service = provider.GetRequiredService<IOpenIdServerService>();
            return new OpenIdSettingsSectionProvider<OpenIdServerSettings>(
                "openid-server", OpenIdConstants.Features.Server, OpenIdPermissions.ManageServerSettings,
                [
                    Choice("accessTokenFormat", ["DataProtection", "JsonWebToken"]),
                    Address("authority"),
                    Boolean("disableAccessTokenEncryption"),
                    Choice("encryptionCertificateStoreLocation", ["CurrentUser", "LocalMachine"], nullable: true),
                    Choice("encryptionCertificateStoreName", ["AddressBook", "AuthRoot", "CertificateAuthority", "Disallowed", "My", "Root", "TrustedPeople", "TrustedPublisher"], nullable: true),
                    Text("encryptionCertificateThumbprint"),
                    Choice("signingCertificateStoreLocation", ["CurrentUser", "LocalMachine"], nullable: true),
                    Choice("signingCertificateStoreName", ["AddressBook", "AuthRoot", "CertificateAuthority", "Disallowed", "My", "Root", "TrustedPeople", "TrustedPublisher"], nullable: true),
                    Text("signingCertificateThumbprint"),
                    Path("authorizationEndpointPath"),
                    Path("logoutEndpointPath"),
                    Path("tokenEndpointPath"),
                    Path("userinfoEndpointPath"),
                    Path("introspectionEndpointPath"),
                    Path("deviceAuthorizationEndpointPath"),
                    Path("endUserVerificationEndpointPath"),
                    Path("pushedAuthorizationEndpointPath"),
                    Path("revocationEndpointPath"),
                    Boolean("allowPasswordFlow"),
                    Boolean("allowClientCredentialsFlow"),
                    Boolean("allowAuthorizationCodeFlow"),
                    Boolean("allowDeviceAuthorizationFlow"),
                    Boolean("allowRefreshTokenFlow"),
                    Boolean("allowHybridFlow"),
                    Boolean("allowImplicitFlow"),
                    Boolean("disableRollingRefreshTokens"),
                    Boolean("useReferenceAccessTokens"),
                    Boolean("requireProofKeyForCodeExchange"),
                    Boolean("requirePushedAuthorizationRequests"),
                    Boolean("requireEndSessionConfirmation"),
                ], service.GetSettingsAsync, service.UpdateSettingsAsync, service.ValidateSettingsAsync,
                provider.GetRequiredService<IShellReleaseManager>());
        });
    }
}

/// <summary>
/// Registers tenant-owned OpenID client settings management.
/// </summary>
[Feature(OpenIdConstants.Features.Client)]
public sealed class OpenIdClientManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider =>
        {
            var service = provider.GetRequiredService<IOpenIdClientService>();
            return new OpenIdSettingsSectionProvider<OpenIdClientSettings>(
                "openid-client", OpenIdConstants.Features.Client, OpenIdPermissions.ManageClientSettings,
                [
                    Text("displayName"),
                    Address("authority"),
                    Text("clientId"),
                    Path("callbackPath"),
                    Text("signedOutRedirectUri"),
                    Path("signedOutCallbackPath"),
                    Text("clientSecret", secret: true),
                    Names("scopes"),
                    Choice("responseType", ["code", "code id_token", "code id_token token", "code token", "id_token", "id_token token"]),
                    Choice("responseMode", ["form_post", "fragment", "query"]),
                    Boolean("storeExternalTokens"),
                    Parameters(),
                ], service.GetSettingsAsync, service.UpdateSettingsAsync, service.ValidateSettingsAsync,
                provider.GetRequiredService<IShellReleaseManager>(), provider.GetRequiredService<IDataProtectionProvider>());
        });
    }
}

/// <summary>
/// Registers tenant-owned OpenID validation settings management.
/// </summary>
[Feature(OpenIdConstants.Features.Validation)]
public sealed class OpenIdValidationManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider =>
        {
            var service = provider.GetRequiredService<IOpenIdValidationService>();
            return new OpenIdSettingsSectionProvider<OpenIdValidationSettings>(
                "openid-validation", OpenIdConstants.Features.Validation, OpenIdPermissions.ManageValidationSettings,
                [
                    Text("audience"),
                    Address("authority"),
                    Text("tenant"),
                    Address("metadataAddress"),
                    Boolean("disableTokenTypeValidation"),
                ], service.GetSettingsAsync, service.UpdateSettingsAsync, service.ValidateSettingsAsync,
                provider.GetRequiredService<IShellReleaseManager>());
        });
    }
}
