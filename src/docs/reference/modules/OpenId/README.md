# OpenID (`OrchardCore.OpenId`)

## OpenID Connect Module

`OrchardCore.OpenId` provides robust OpenID Connect functionality, enabling Orchard Core to act as an OpenID Connect server and client. The following features are available:

- **OpenID Connect Core Services**
- **OpenID Connect Authorization Server**
- **OpenID Connect Management UI**
- **OpenID Connect Authorization Server**
- **OpenID Connect Client Integration**

## Remote management authentication reference

See [Management API authentication](../../api/authentication/README.md) for the OpenID Connect endpoints, grants, scopes, and request formats used by remote management clients.

## Core OpenID Connect Services

This feature provides the essential services that underpin all other OpenID Connect features within Orchard Core. It includes fundamental components needed for secure communication, token handling, and user authentication.

## OpenID Connect Management UI

Adds a management interface to the Orchard Core admin dashboard, enabling users to manage OpenID Connect applications, define and modify scopes, and configure application permissions through a user-friendly UI.

## OpenID Connect Authorization Server

Allows Orchard Core to function as an OpenID Connect authorization server, also referred to as an identity provider (IdP). This feature enables authentication and the issuance of tokens, conforming to OpenID Connect and OAuth 2.0 standards.

Key points:
- Orchard Core can serve as a centralized identity provider, allowing external applications to authenticate users and manage access control.
- Powered by the [`OpenIddict`](https://github.com/openiddict/openiddict-core) library, this feature supports token-based authentication without requiring an external identity provider.
- The authorization server takes care of validating the access tokens received by the `/connect/userinfo` API endpoint, so you don't need to enable the token validation feature for the current tenant.
- To validate issued tokens, ensure the 'OpenID Connect Token Validation' feature is activated.

Supported flows include:
- [Authorization Code Flow](http://openid.net/specs/openid-connect-core-1_0.html)
- [Device Authorization Grant](https://datatracker.ietf.org/doc/html/rfc8628)
- [Implicit Flow](http://openid.net/specs/openid-connect-core-1_0.html)
- [Hybrid Flow](http://openid.net/specs/openid-connect-core-1_0.html)
- [Client Credentials Grant](https://tools.ietf.org/html/rfc6749)
- [Resource Owner Password Grant](https://tools.ietf.org/html/rfc6749)

### Customizing authorization pages

The authorization server's consent, device verification, sign-out, and error
pages use the same theme selection as the login page. By default, they use the
tenant's admin theme and its `Layout__Login` layout. The default consent view
shows readable permission labels alongside the exact scope identifiers, with
localized **Allow access** and **Cancel** actions.

To use your site's branding, activate a site theme and enable **Use site theme
for login page** in the user login settings. This sets
`LoginSettings.UseSiteTheme` to `true`. The admin theme remains the fallback
when no site theme is selected.

The SaaS theme focuses **Allow access** when the browser consent page opens,
so pressing Enter approves the request. Tab moves focus to **Deny**.

Its green accents, cream background, and serif headings take inspiration from
[orchardcore.net](https://orchardcore.net/). It follows the browser's light or
dark color preference and uses locally available fonts. To adjust this palette
in a derived theme, override the `--saas-*` CSS custom properties, including
`--saas-primary`, `--saas-surface`, `--saas-ink`, and `--saas-heading-font`,
after the theme stylesheet. Provide corresponding dark-mode overrides as needed.
The decorative leaf background is the `SaaSBackground` shape. Override
`Views/SaaSBackground.cshtml` in a derived theme to replace it, or set
`--saas-background-opacity: 0` to hide it. The leaf is inline SVG and requires
no image download.
See the [SaaS consent example](../../../guides/remote-management/README.md#4-sign-in-once).

You can also select an installed site theme through the
[theme management CLI/API](../../api/themes/README.md#set-the-current-theme):

```bash
pomi themes list --admin false
pomi themes set-current TheAgencyTheme
```

Replace `TheAgencyTheme` with the theme ID returned by the list command. The
selection operation also enables the theme and its base themes.

The login-theme flag itself is not exposed by `pomi settings update` or
`PUT /api/settings`, which only accept
[safe core site settings](../../api/settings/README.md#site-settings-representation).
For automation, include the
[login settings recipe configuration](../Users/README.md#login-settings)
in an installed non-setup recipe, then run it through the
[recipe management API](../../api/recipes/README.md#execute-a-recipe)
or its `pomi recipes execute <recipeId> --json '{}' --force` CLI command, using an ID
returned by `pomi recipes list`.
That API executes installed recipes; it does not accept a new recipe body.
The `Settings` recipe step replaces the supplied `LoginSettings` section, so
include the other login settings you intend to retain.

A theme can provide `Views/Layout-Login.cshtml` to customize the surrounding
layout. To replace individual MVC views, add the corresponding file to the
active theme:

| Theme file | Purpose |
| --- | --- |
| `Views/OrchardCore.OpenId/Access/Authorize.cshtml` | Browser consent |
| `Views/OrchardCore.OpenId/Access/Verify.cshtml` | Device code entry and consent |
| `Views/OrchardCore.OpenId/Access/Logout.cshtml` | Sign-out confirmation |
| `Views/OrchardCore.OpenId/Access/Error.cshtml` | Authorization error |

#### Consent scope shape

Both consent views render the `OpenIdConsentScopes` shape through Orchard's
display pipeline. Its `Model.Scope` property contains the space-separated
requested scope identifiers. The default binding is
`Views/OpenIdConsentScopes.cshtml` in the OpenID module.

A theme can override this binding with `Views/OpenIdConsentScopes.cshtml` or
`Views/OpenIdConsentScopes.liquid`. Other modules can customize it using an
`IShapeTableProvider` that describes `OpenIdConsentScopes`, or supply a shape
template through the usual feature dependency and binding precedence rules.

With the [Templates module](../Templates/README.md) enabled, create a dynamic
Liquid template named `OpenIdConsentScopes`. For example:

```liquid
{% assign scopes = Model.Scope | split: ' ' | uniq %}
{% if scopes.size > 0 %}
  <h2>{{ 'Requested access' | t }}</h2>
  <ul>
    {% for scope in scopes %}
      <li><code>{{ scope | escape }}</code></li>
    {% endfor %}
  </ul>
{% endif %}
```

The [Templates management CLI/API](../../api/templates/README.md#create-a-template)
can also create or update that dynamic template. This scope-list shape works
on the consent pages even when they use the admin theme: selecting that theme
does not make these pages admin routes. The surrounding forms continue to
handle protocol parameters and the approval/cancellation actions.

#### Custom form views

Start from the module's view when overriding a form. Preserve its model,
protocol parameters, form action and method, antiforgery token, and submit
button names (`submit.Accept` and `submit.Deny`) with nonempty values. Keep
the application name, requested permissions, and device-code comparison
visible so users can make an informed choice. Continue using Razor's encoded
output for application names, scope identifiers, and protocol values.

The default views use Bootstrap 5 classes. Themes using another CSS framework
can replace the views as well as the layout. No OAuth endpoint or CLI change
is needed to customize their appearance.

## OpenID Connect Token Validation

This feature is responsible for validating tokens issued either by Orchard Core's own OpenID Connect authorization server or by other trusted servers. It supports JSON Web Tokens (JWT) and OpenID Connect discovery, ensuring secure and reliable token validation across distributed applications.

### Configuration

Configuration can be set through the _OpenID Connect_ settings menu in the admin dashboard and also through a recipe step.

Available settings are:

- Token Format:
  - Data Protection: this format - enabled by default - uses non-standard opaque tokens encrypted by the ASP.NET Core Data Protection stack.
  - Json Web Token: this format uses signed JWT standard tokens. The tokens are encrypted by default but access token encryption can be turned off
to allow third-party resource servers to use the JWT tokens produced by the Orchard OpenID server.
- Authority: Orchard URL used by Orchard to act as an identity server.
- Signing Certificate Store Location: CurrentUser/LocalMachine <https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx>
- Signing Certificate Store Name: AddressBook/AuthRootCertificateAuthority/Disallowed/My/Root/TrustedPeople/TrustedPublisher <https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx>
- Encryption Certificate Thumbprint: the thumbprint of the signing certificate (it is recommended to not use same certificate that is being used for SSL).
- Encryption Certificate Store Location: CurrentUser/LocalMachine <https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx>
- Encryption Certificate Store Name: AddressBook/AuthRootCertificateAuthority/Disallowed/My/Root/TrustedPeople/TrustedPublisher <https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx>
- Encryption Certificate Thumbprint: the thumbprint of the encryption certificate (it is recommended to not use same certificate that is being used for SSL).
- Enable Token Endpoint.
- Enable Authorization Endpoint.
- Enable Logout Endpoint.
- Enable User Info Endpoint.
- Enable Device Authorization Endpoint.
- Enable End-User Verification Endpoint.
- Allow Password Flow: It requires that the Token Endpoint is enabled. More info at <https://tools.ietf.org/html/rfc6749#section-1.3.3>
- Allow Client Credentials Flow: It requires that the Token Endpoint is enabled. More info at <https://tools.ietf.org/html/rfc6749#section-1.3.4>
- Allow Authorization Code Flow: It requires that the Authorization and Token Endpoints are enabled. More info at <http://openid.net/specs/openid-connect-core-1_0.html#CodeFlowAuth>
- Allow Device Authorization Flow: It requires that the Device Authorization, End-User Verification, and Token Endpoints are enabled. More info at <https://datatracker.ietf.org/doc/html/rfc8628>
- Allow Implicit Flow: It requires that the Authorization Endpoint is enabled. More info at <http://openid.net/specs/openid-connect-core-1_0.html#ImplicitFlowAuth>
- Allow Refresh Token Flow: It allows refreshing the access token using a refresh token. It can be used in combination with Password Flow, Authorization Code Flow, Device Authorization Flow, and Hybrid Flow. More info at <http://openid.net/specs/openid-connect-core-1_0.html#RefreshTokens>
- Require Proof Key for Code Exchange: Global setting that applies PKCE to all registered clients whether or not the 'Require PKCE' flag was set in the Application settings page.

A sample of OpenID Connect Settings recipe step:

```json
{
      "name": "OpenIdServerSettings",
      "TestingModeEnabled": false,
      "AccessTokenFormat": "JsonWebToken", // JsonWebToken or DataProtection
      "Authority": "https://www.orchardproject.net",
      "SigningCertificateStoreLocation": "LocalMachine", //More info: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx
      "SigningCertificateStoreName": "My", //More info: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx
      "SigningCertificateThumbprint": "27CCA66EF38EF46CD9022431FB1FF0F2DF5CA1D7",
      "EncryptionCertificateStoreLocation": "LocalMachine",
      "EncryptionCertificateStoreName": "My",
      "EncryptionCertificateThumbprint": "BC34460ABEA2D576EA68E8FFCFEEB3F45C94FB0F",
      "EnableTokenEndpoint": true,
      "EnableAuthorizationEndpoint": false,
      "EnableDeviceAuthorizationEndpoint": true,
      "EnableEndUserVerificationEndpoint": true,
      "EnableIntrospectionEndpoint": false,
      "EnableLogoutEndpoint": true,
      "EnablePushedAuthorizationEndpoint": false,
      "EnableRevocationEndpoint": false,
      "EnableUserInfoEndpoint": true,
      "AllowPasswordFlow": true,
      "AllowClientCredentialsFlow": false,
      "AllowAuthorizationCodeFlow": false,
      "AllowDeviceAuthorizationFlow": false,
      "AllowRefreshTokenFlow": false,
      "AllowImplicitFlow": false,
      "RequireProofKeyForCodeExchange": false,
      "RequirePushedAuthorizationRequests": false
}
```

### OpenID Connect Client Integration Configuration

OpenID Connect apps can be set through OpenID Connect Apps menu in the admin dashboard (through the Management Interface feature)
and also through a recipe step.

OpenID Connect apps require the following configuration.

- Id: Unique identifier.
- Client Id: Client identifier of the application. It has to be provided by a client when requesting a valid token.
- Display Name: Display name associated with the current application.
- Application Type:
  - Web application: Uses fixed redirect URIs.
  - Native application: Represents an installed application and permits loopback redirect URIs with an ephemeral port. Native applications must use the Public client type.
- Type: There are two options:
  - Confidential: Confidential applications MUST send their client secret when communicating with the token and revocation endpoints. This guarantees that only the legit client can exchange an authorization code or get a refresh token.
  - Public: Public applications don't use client secret on their communications.
- Client Secret: Client secret is a password associated with the application. It will be required when the application is configured as Confidential.
- Flows: If general OpenID Connect settings allow this flow, an app can also enable this flow.
  - Allow Password Flow: It requires that the Token Endpoint is enabled. More info at <https://tools.ietf.org/html/rfc6749#section-1.3.3>
  - Allow Client Credentials Flow: It requires that the Token Endpoint is enabled. More info at <https://tools.ietf.org/html/rfc6749#section-1.3.4>
  - Allow Authorization Code Flow: It requires that the Authorization and Token Endpoints are enabled. More info at <http://openid.net/specs/openid-connect-core-1_0.html#CodeFlowAuth>
- Allow Device Authorization Flow: It requires that the Device Authorization endpoint is enabled. More info at <https://datatracker.ietf.org/doc/html/rfc8628>
  - Allow Implicit Flow: It requires that the Authorization Endpoint is enabled. More info at <http://openid.net/specs/openid-connect-core-1_0.html#ImplicitFlowAuth>
  - Allow Refresh Token Flow: It allows refreshing the access token using a refresh token. It can be used in combination with Password Flow, Authorization Code Flow, Device Authorization Flow, and Hybrid Flow. More info at <http://openid.net/specs/openid-connect-core-1_0.html#RefreshTokens>
- Normalized RoleNames: This configuration is only required if Client Credentials Flow is enabled. It determines the roles assigned to the app when it is authenticated using that flow.
- Redirect Options: Those options are only required when Implicit Flow, Authorization Code Flow or Allow Hybrid Flow is required.
- Logout Redirect Uri: logout callback URL.
- Redirect Uri: callback URL.
- Skip Consent: sets whether a consent form has to be completed by the user after log in.
- Advanced Parameters: Allows setting additional parameters that can be sent with the authorize request. Note: The default parameters are set from the options above.
- Require PKCE: Applies PKCE for the registered application. Ensure that the client library being used supports PKCE.

The application editor and recipe step share the same settings update logic. Updating a
confidential application without a new client secret preserves its existing credential.
Switching to a public application removes the secret. Roles, scopes and redirect URIs
replace their existing collections, while custom descriptor properties are preserved.
If OpenID validation rejects an update, the previous application values remain available.

A sample of OpenID Connect App recipe step:

```json
{
      "name": "openidapplication",
      "ClientId": "openidtest",
      "DisplayName": "Open Id Test",
      "ApplicationType": "web",
      "Type": "Confidential",
      "ClientSecret": "MyPassword",
      "EnableTokenEndpoint": true,
      "EnableAuthorizationEndpoint": false,
      "EnableLogoutEndpoint": true,
      "EnableUserInfoEndpoint": true,
      "AllowPasswordFlow": true,
      "AllowClientCredentialsFlow": false,
      "AllowAuthorizationCodeFlow": false,
      "AllowDeviceAuthorizationFlow": false,
      "AllowRefreshTokenFlow": false,
      "AllowImplicitFlow": false,
      "RequireProofKeyForCodeExchange": false,
      "RequirePushedAuthorizationRequests": false
}
```

### OpenID Connect Scopes Configuration

Scopes can be set through OpenID Connect Scopes menu in the admin dashboard (through the Management Interface feature) and also through a recipe step.

OpenID Connect Scopes require the following configuration.

| Property             | Description                                                       |
|----------------------|-------------------------------------------------------------------|
| Name                 | Unique name of the scope.                                         |
| Display Name         | Display name associated with the current scope.                   |
| Description          | Describe how this scope is used in the system.                    |
| Tenants              | Build the audience based on tenants names.                        |
| Additional resources | Build the audience based on the space separated strings provided. |

Admin scope edits replace the resources list, so an empty field clears it. A scope
recipe update that omits `Resources` or supplies an empty string preserves the
existing resources; a nonempty value replaces them. Both paths preserve custom
properties they do not edit and skip saving when their editable values already
match. The recipe step uses the same descriptor update as the admin editor.

A sample of OpenID Connect Scope recipe step:

```json
    {
      "name": "OpenIdScope",
      "Description": "A scope to provide audience for remote clients",
      "DisplayName": "External Audience Scope",
      "ScopeName": "custom_scope",
      "Resources": "my_recipient"
    }
```

### Configuring Certificates

#### Windows / IIS

Several tools are available for generating a signing certificate on Windows and/or IIS, for example:

- IIS Server Manager _(offers limited control)_
    1. Server Certificates
    2. Create Self-Signed Certificate
- PowerShell _(offers full control)_
    1. `New-SelfSignedCertificate`, for example:

```powershell
# See https://technet.microsoft.com/en-us/itpro/powershell/windows/pkiclient/new-selfsignedcertificate

New-SelfSignedCertificate `
    -Subject "connect.example.com" `
    -FriendlyName "Example.com Signing Certificate" `
    -CertStoreLocation "cert:\LocalMachine\My" `
    -KeySpec Signature `
    -KeyUsage DigitalSignature `
    -KeyUsageProperty Sign `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
    -KeyExportPolicy NonExportable `
    -KeyAlgorithm RSA `
    -KeyLength 4096 `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddDays(825) `
    -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider"
```

**This snippet must be run as admin.** It generates a 4096-bit signing certificate, stores it in the machine store and returns the certificate's thumbprint, which you need in the OpenID Connect Settings recipe or when exporting the certificate through PowerShell. _You should update this example according to your requirements!_

In multi-node environments consider creating the certificate with `-KeyExportPolicy Exportable`, then export the certificate (PFX) to a secure location, using the MMC Certificates Snap-In or PowerShell `Export-PfxCertificate`, and subsequently import the certificate on each node as non-exportable, which is the default when using `Import-PfxCertificate`. For example:

```powershell
# See https://technet.microsoft.com/en-us/itpro/powershell/windows/pkiclient/export-pfxcertificate
# Run this on the machine where the certificate was generated:

$mypwd = ConvertTo-SecureString -String "MySecretPassword123" -Force -AsPlainText

Export-PfxCertificate -FilePath C:\securelocation\connect.example.com.pfx cert:\localMachine\my\thumbprintfromnewselfsignedcertificate -Password $mypwd

# See https://technet.microsoft.com/en-us/itpro/powershell/windows/pkiclient/import-pfxcertificate
# Run this on the target node:

$mypwd = ConvertTo-SecureString -String "MySecretPassword123" -Force -AsPlainText

Import-PfxCertificate -FilePath C:\securelocation\connect.example.com.pfx cert:\localMachine\my -Password $mypwd
```

**Important:** In order for the `OrchardCore.OpenId` module to use the certificate's keys for signing, it requires `Read` access to the certificate in the store. This can be granted in various ways, for example:

- `MMC.exe`
    1. Add Snap-In 'Certificates' for Computer Account
    2. Right-Click relevant certificate and select All Tasks, Manage Private Keys
    3. Add the relevant identity (e.g. IIS AppPool\PoolName)
        - Add
        - Advanced
        - Locations: Choose iis server machine name
        - Find Now
        - Search Results: Choose your iisServerMachineName\IIS_IUSRS (just one example)
        - OK
    4. Check Allow Read under Permissions
- `WinHttpCertCfg.exe` (grants Full Control)
    1. For example: `winhttpcertcfg -g -c LOCAL_MACHINE\My -s connect.example.com -a AppPoolIdentityName` <https://msdn.microsoft.com/en-us/library/windows/desktop/aa384088(v=vs.85).aspx>

### Use the Certificate in Azure

To use the certificate on an Azure hosted site.

1. Upload the certificate to the 'TLS/SSL settings' page of the site Azure portal page.
2. Add a new entry to the Azure site setting page with the following:
    - Key: WEBSITE_LOAD_CERTIFICATES
    - Value: [Thumbprint of the certificate]
3. Select the certificate under `CurrentUser` > `My` certificate store.

## Token Validation

- Validates tokens issued by the Orchard OpenID server
  - Configure the validation feature to transparently use the server configuration of another tenant, which has the authorization server feature enabled.
- Validates token by a remote server supporting JWT and OpenID Connect discovery.

Token Validation require the following configuration.

| Property                    | Description                                                                                                      |
|-----------------------------|------------------------------------------------------------------------------------------------------------------|
| Authorization server tenant | The tenant that runs OpenID Connect Server. If none is selected, then the following properties must be provided. |
| Authority                   | The address of the remote OpenID Connect server that issued the token.                                           |
| Audience                    | Defines the intended recipient of the token that must be checked.                                                |

A sample of Token Validation Settings recipe step:

```json
    {
      "name": "OpenIdValidationSettings",
      "Audience": "my_recipient",
      "Authority": "https://idp.domain.com"
    }
```

## OIDC Client

Authenticates users from an external OpenID Connect identity provider.
If the site allows new users to register, a local user and the external login are linked.
If an "email" claim is received, and a local user is found, then the external login is linked to that account, after authenticating.

### OpenId Configuration

Configuration can be set through the _OpenID Connect_ settings menu in the admin dashboard and also through a recipe step.

Available settings are:

- Display Name: Display name of the IdP. It is shown in the login form.
- Authority: Authority to use when making OpenIdConnect calls.
- ClientId: The `client_id` part of the query.
- CallbackPath: The request path within the application's base path where the user agent will be returned after sign out from the identity provider. See `post_logout_redirect_uri` from <http://openid.net/specs/openid-connect-session-1_0.html#RedirectionAfterLogout>
- SignedOut CallbackPath: the callback endpoint for signout. Defaults to `/signout-callback-oidc`.
- SignedOut Redirect Uri: The URI where the user agent will be redirected to after application is signed out from the identity provider. The redirect will happen after the `SignedOutCallbackPath` is invoked.
- Scopes: Extra scopes except openid and profile.
- Response Mode: Configure Response Mode see: <http://openid.net/specs/openid-connect-core-1_0.html#ImplicitAuthResponse>. If fragment or query only Code Authentication Flow is allowed.
- Supported Flows: Select one of the OIDC flows:
  - Code Authentication Flow (see: <http://openid.net/specs/openid-connect-core-1_0.html#CodeFlowAuth>)
  - Hybrid Authentication Flow (see: <http://openid.net/specs/openid-connect-core-1_0.html#HybridAuthRequest>)
    - Use `code id_token` response type (example: <http://openid.net/specs/openid-connect-core-1_0.html#code-id_token-tokenExample>)
    - Use `code id_token token` response type (example: <http://openid.net/specs/openid-connect-core-1_0.html#code-id_token-tokenExample>)
    - Use `code token` response type (example: <http://openid.net/specs/openid-connect-core-1_0.html#code-tokenExample>)
  - Implicit Authentication Flow (see: <http://openid.net/specs/openid-connect-core-1_0.html#ImplicitAuthRequest>)
    - Use `id_token` response type (example: <http://openid.net/specs/openid-connect-core-1_0.html#id_tokenExample>)
    - Use `id_token token` response type (example: <http://openid.net/specs/openid-connect-core-1_0.html#id_token-tokenExample>)
- Client Secret: It is used with one of the 'confidential' flows, code or hybrid.

A sample of OpenID Connect Client Settings recipe step:

```json
{
      "name": "OpenIdClientSettings",
      "Authority": "http://localhost:44300/t1",
      "DisplayName": "Orchard (t1) IdP",
      "ClientId": "orchard_t2", 
      "CallbackPath": "/signin-oidc",
      "SignedOutCallbackPath": "/signout-callback-oidc",
      "Scopes": "email phone",
      "ResponseMode": "form_post",
      "ResponseType": "code id_token",
      "ClientSecret": "secret"
}
```

## Settings Recipe Step

All OpenID Connect settings can be configured using the generic `Settings` recipe step:

### Server Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "OpenIdServerSettings": {
        "TestingModeEnabled": false,
        "TokenFormat": "JsonWebToken",
        "Authority": "https://www.example.com",
        "AuthorizationEndpointPath": "/connect/authorize",
        "LogoutEndpointPath": "/connect/logout",
        "TokenEndpointPath": "/connect/token",
        "UserinfoEndpointPath": "/connect/userinfo",
        "IntrospectionEndpointPath": "/connect/introspect",
        "RevocationEndpointPath": "/connect/revoke",
        "EnableTokenEndpoint": true,
        "EnableAuthorizationEndpoint": true,
        "EnableLogoutEndpoint": true,
        "EnableUserInfoEndpoint": true,
        "EnableIntrospectionEndpoint": false,
        "EnableRevocationEndpoint": false,
        "AllowPasswordFlow": false,
        "AllowClientCredentialsFlow": false,
        "AllowAuthorizationCodeFlow": true,
        "AllowRefreshTokenFlow": true,
        "AllowImplicitFlow": false,
        "AllowHybridFlow": false,
        "RequireProofKeyForCodeExchange": true,
        "UseRollingRefreshTokens": false,
        "UseReferenceAccessTokens": false
      }
    }
  ]
}
```

| Property                         | Type    | Description                                                               |
|----------------------------------|---------|---------------------------------------------------------------------------|
| `TestingModeEnabled`             | Boolean | Whether testing mode is enabled (uses ephemeral signing/encryption keys). |
| `TokenFormat`                    | String  | The access token format. Values: `DataProtection`, `JsonWebToken`.        |
| `Authority`                      | String  | The authority URL used by Orchard to act as an identity server.           |
| `AuthorizationEndpointPath`      | String  | Path for the authorization endpoint.                                      |
| `LogoutEndpointPath`             | String  | Path for the logout endpoint.                                             |
| `TokenEndpointPath`              | String  | Path for the token endpoint.                                              |
| `UserinfoEndpointPath`           | String  | Path for the userinfo endpoint.                                           |
| `IntrospectionEndpointPath`      | String  | Path for the introspection endpoint.                                      |
| `RevocationEndpointPath`         | String  | Path for the revocation endpoint.                                         |
| `EnableTokenEndpoint`            | Boolean | Whether the token endpoint is enabled.                                    |
| `EnableAuthorizationEndpoint`    | Boolean | Whether the authorization endpoint is enabled.                            |
| `EnableLogoutEndpoint`           | Boolean | Whether the logout endpoint is enabled.                                   |
| `EnableUserInfoEndpoint`         | Boolean | Whether the userinfo endpoint is enabled.                                 |
| `EnableIntrospectionEndpoint`    | Boolean | Whether the introspection endpoint is enabled.                            |
| `EnableRevocationEndpoint`       | Boolean | Whether the revocation endpoint is enabled.                               |
| `AllowPasswordFlow`              | Boolean | Whether the Resource Owner Password flow is allowed.                      |
| `AllowClientCredentialsFlow`     | Boolean | Whether the Client Credentials flow is allowed.                           |
| `AllowAuthorizationCodeFlow`     | Boolean | Whether the Authorization Code flow is allowed.                           |
| `AllowRefreshTokenFlow`          | Boolean | Whether the Refresh Token flow is allowed.                                |
| `AllowImplicitFlow`              | Boolean | Whether the Implicit flow is allowed.                                     |
| `AllowHybridFlow`                | Boolean | Whether the Hybrid flow is allowed.                                       |
| `RequireProofKeyForCodeExchange` | Boolean | Whether PKCE is required for all clients.                                 |
| `UseRollingRefreshTokens`        | Boolean | Whether to use rolling refresh tokens.                                    |
| `UseReferenceAccessTokens`       | Boolean | Whether to use reference access tokens.                                   |

### Client Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "OpenIdClientSettings": {
        "DisplayName": "External Identity Provider",
        "Authority": "https://idp.example.com",
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "CallbackPath": "/signin-oidc",
        "SignedOutCallbackPath": "/signout-callback-oidc",
        "ResponseType": "code",
        "ResponseMode": "form_post",
        "Scopes": "openid profile email"
      }
    }
  ]
}
```

| Property                | Type   | Description                                                                                                                      |
|-------------------------|--------|----------------------------------------------------------------------------------------------------------------------------------|
| `DisplayName`           | String | The display name for the external identity provider.                                                                             |
| `Authority`             | String | The authority URL of the OpenID Connect provider. **Required.**                                                                  |
| `ClientId`              | String | The client identifier. **Required.**                                                                                             |
| `ClientSecret`          | String | The client secret.                                                                                                               |
| `CallbackPath`          | String | The callback path where the user-agent will be returned.                                                                         |
| `SignedOutCallbackPath` | String | The callback path after sign-out.                                                                                                |
| `SignedOutRedirectUri`  | String | The URI to redirect to after sign-out.                                                                                           |
| `ResponseType`          | String | The OAuth 2.0 response type. Values: `code`, `id_token`, `id_token token`, `code id_token`, `code token`, `code id_token token`. |
| `ResponseMode`          | String | The response mode. Values: `form_post`, `fragment`, `query`.                                                                     |
| `Scopes`                | String | Space-separated list of scopes to request.                                                                                       |

### Validation Settings

```json
{
  "steps": [
    {
      "name": "settings",
      "OpenIdValidationSettings": {
        "Audience": "your-resource-server",
        "Authority": "https://idp.example.com",
        "DisableTokenTypeValidation": false,
        "Tenant": "",
        "MetadataAddress": ""
      }
    }
  ]
}
```

| Property                     | Type    | Description                                                           |
|------------------------------|---------|-----------------------------------------------------------------------|
| `Audience`                   | String  | The audience used for token validation.                               |
| `Authority`                  | String  | The authority URL for OpenID Connect discovery.                       |
| `DisableTokenTypeValidation` | Boolean | Whether to disable access token type validation.                      |
| `Tenant`                     | String  | The Orchard tenant for local server validation.                       |
| `MetadataAddress`            | String  | Override the metadata discovery address (for non-standard providers). |

## Remote application and scope administration

With `OrchardCore.OpenId.Management` enabled, authorized clients can manage
applications through `pomi openid applications` and manage scope definitions through
`pomi openid scopes`. Reads use the same managers as the admin UI and omit
credentials, keys and private properties. Application and scope mutations share
descriptor updates with their admin editors and recipe steps. See the [OpenID management API](../../api/openid/README.md)
for paging, permissions and response fields. Confidential application secrets can be
rotated or revoked through `pomi openid applications credentials`. Rotation requires
a new private `--secret-output-file` and immediately replaces the previous secret.

## Remote configuration

The owning OpenID features contribute three typed settings sections to remote
management. Use `pomi settings sections schema <section>` to inspect the complete
update contract, `show <section>` to read it, and `update <section> --stdin` to
apply a JSON patch. The same operations are available through HTTP and MCP.

| Section | Feature | Required permission | Managed tenant settings |
| --- | --- | --- | --- |
| `openid-server` | `OrchardCore.OpenId.Server` | `ManageServerSettings` | Endpoint paths, supported grant flows, access-token format/encryption, PKCE/PAR requirements, refresh-token behavior, and certificate-store selections |
| `openid-client` | `OrchardCore.OpenId.Client` | `ManageClientSettings` | Authority, client ID/secret, callback paths, response type/mode, scopes, external-token storage, and extra authentication parameters |
| `openid-validation` | `OrchardCore.OpenId.Validation` | `ManageValidationSettings` | Local server tenant or remote authority/audience/metadata address, and token-type validation |

All sections also require remote-management access. Writes require HTTPS. They
use the same validation and persistence services as the existing OpenID admin
configuration and request a tenant reload only when values change. Omitted
properties are preserved and supplied arrays replace previous arrays. Unknown
properties and values outside the section schema are rejected before persistence.
Existing flow/endpoint and tenant/authority constraints still apply.

The client secret is encrypted with the existing OpenID client data-protection
purpose. It and extra authentication parameters are omitted from readback;
`hasClientSecret` and `hasParameters` report presence. Omitting either property
retains it; explicitly setting it to null clears it. Sending the same secret again
does not re-encrypt it or trigger a reload. Send secrets through private JSON
input, not shell arguments. Parameters follow existing client storage behavior;
redaction does not encrypt their values.

These sections manage the tenant's saved configuration. They do not modify host
configuration, custom application option overrides, certificate files/private
keys, provider-specific external login settings, application registrations, or
user MFA enrollment. Certificate settings select existing certificates or preserve
the module's managed-certificate fallback. Applications and scopes retain their
separate management commands.

Changing token endpoints, enabled flows or validation authority can invalidate the
context used to make the change. Keep a working administrative recovery path and
refresh Pomi's API/context configuration after such changes.
