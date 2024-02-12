using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Server.DataProtection;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Configuration
{
    [Feature(OpenIdConstants.Features.Server)]
    public class OpenIdServerConfiguration : IConfigureOptions<AuthenticationOptions>,
        IConfigureOptions<OpenIddictServerOptions>,
        IConfigureOptions<OpenIddictServerDataProtectionOptions>,
        IConfigureNamedOptions<OpenIddictServerAspNetCoreOptions>
    {
        private readonly IOpenIdServerService _serverService;
        private readonly OpenIdServerSettings _openIdServerSettings;
        private readonly ILogger _logger;

        public OpenIdServerConfiguration(
            IOpenIdServerService serverService,
            IOptions<OpenIdServerSettings> openIdServerSettings,
            ILogger<OpenIdServerConfiguration> logger)
        {
            _serverService = serverService;
            _openIdServerSettings = openIdServerSettings.Value;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_openIdServerSettings == null)
            {
                return;
            }

            options.AddScheme<OpenIddictServerAspNetCoreHandler>(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, displayName: null);
        }

        public void Configure(OpenIddictServerOptions options)
        {
            if (_openIdServerSettings == null)
            {
                return;
            }

            options.Issuer = _openIdServerSettings.Authority;
            options.DisableAccessTokenEncryption = _openIdServerSettings.DisableAccessTokenEncryption;
            options.DisableRollingRefreshTokens = _openIdServerSettings.DisableRollingRefreshTokens;
            options.UseReferenceAccessTokens = _openIdServerSettings.UseReferenceAccessTokens;

            foreach (var key in _serverService.GetEncryptionKeysAsync().GetAwaiter().GetResult())
            {
                options.EncryptionCredentials.Add(new EncryptingCredentials(key,
                    SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512));
            }

            foreach (var key in _serverService.GetSigningKeysAsync().GetAwaiter().GetResult())
            {
                options.SigningCredentials.Add(new SigningCredentials(key, SecurityAlgorithms.RsaSha256));
            }

            // Note: while endpoint paths in OrchardCore are stored as PathString instances,
            // OpenIddict uses System.Uri. To ensure the System.Uri instances created from
            // a PathString don't represent root-relative URIs (which would break path-based
            // multi-tenancy support), the leading '/' that is always present in PathString
            // instances is manually removed from the endpoint path before URIs are created.

            if (_openIdServerSettings.AuthorizationEndpointPath.HasValue)
            {
                options.AuthorizationEndpointUris.Add(new Uri(
                    _openIdServerSettings.AuthorizationEndpointPath.ToUriComponent()[1..], UriKind.Relative));
            }

            if (_openIdServerSettings.LogoutEndpointPath.HasValue)
            {
                options.LogoutEndpointUris.Add(new Uri(
                    _openIdServerSettings.LogoutEndpointPath.ToUriComponent()[1..], UriKind.Relative));
            }

            if (_openIdServerSettings.TokenEndpointPath.HasValue)
            {
                options.TokenEndpointUris.Add(new Uri(
                    _openIdServerSettings.TokenEndpointPath.ToUriComponent()[1..], UriKind.Relative));
            }

            if (_openIdServerSettings.UserinfoEndpointPath.HasValue)
            {
                options.UserinfoEndpointUris.Add(new Uri(
                    _openIdServerSettings.UserinfoEndpointPath.ToUriComponent()[1..], UriKind.Relative));
            }

            if (_openIdServerSettings.IntrospectionEndpointPath.HasValue)
            {
                options.IntrospectionEndpointUris.Add(new Uri(
                    _openIdServerSettings.IntrospectionEndpointPath.ToUriComponent()[1..], UriKind.Relative));
            }

            if (_openIdServerSettings.RevocationEndpointPath.HasValue)
            {
                options.RevocationEndpointUris.Add(new Uri(
                    _openIdServerSettings.RevocationEndpointPath.ToUriComponent()[1..], UriKind.Relative));
            }

            // For now, response types and response modes are not directly
            // configurable and are inferred from the selected flows.
            if (_openIdServerSettings.AllowAuthorizationCodeFlow)
            {
                options.CodeChallengeMethods.Add(CodeChallengeMethods.Plain);
                options.CodeChallengeMethods.Add(CodeChallengeMethods.Sha256);

                options.GrantTypes.Add(GrantTypes.AuthorizationCode);

                options.ResponseModes.Add(ResponseModes.FormPost);
                options.ResponseModes.Add(ResponseModes.Fragment);
                options.ResponseModes.Add(ResponseModes.Query);

                options.ResponseTypes.Add(ResponseTypes.Code);
            }

            if (_openIdServerSettings.AllowClientCredentialsFlow)
            {
                options.GrantTypes.Add(GrantTypes.ClientCredentials);
            }

            if (_openIdServerSettings.AllowHybridFlow)
            {
                options.CodeChallengeMethods.Add(CodeChallengeMethods.Plain);
                options.CodeChallengeMethods.Add(CodeChallengeMethods.Sha256);

                options.GrantTypes.Add(GrantTypes.AuthorizationCode);
                options.GrantTypes.Add(GrantTypes.Implicit);

                options.ResponseModes.Add(ResponseModes.FormPost);
                options.ResponseModes.Add(ResponseModes.Fragment);

                options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.IdToken);
                options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.IdToken + ' ' + ResponseTypes.Token);
                options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.Token);
            }

            if (_openIdServerSettings.AllowImplicitFlow)
            {
                options.GrantTypes.Add(GrantTypes.Implicit);

                options.ResponseModes.Add(ResponseModes.FormPost);
                options.ResponseModes.Add(ResponseModes.Fragment);

                options.ResponseTypes.Add(ResponseTypes.IdToken);
                options.ResponseTypes.Add(ResponseTypes.IdToken + ' ' + ResponseTypes.Token);
                options.ResponseTypes.Add(ResponseTypes.Token);
            }

            if (_openIdServerSettings.AllowPasswordFlow)
            {
                options.GrantTypes.Add(GrantTypes.Password);
            }

            if (_openIdServerSettings.AllowRefreshTokenFlow)
            {
                options.GrantTypes.Add(GrantTypes.RefreshToken);

                options.Scopes.Add(Scopes.OfflineAccess);
            }

            options.RequireProofKeyForCodeExchange = _openIdServerSettings.RequireProofKeyForCodeExchange;

            options.Scopes.Add(Scopes.Email);
            options.Scopes.Add(Scopes.Phone);
            options.Scopes.Add(Scopes.Profile);
            options.Scopes.Add(Scopes.Roles);
        }

        public void Configure(OpenIddictServerDataProtectionOptions options)
        {
            if (_openIdServerSettings == null)
            {
                return;
            }

            // All the tokens produced by the server feature use ASP.NET Core Data Protection as the default
            // token format, but an option is provided to allow switching to JWT for access tokens only.
            options.PreferDefaultAccessTokenFormat = _openIdServerSettings.AccessTokenFormat == OpenIdServerSettings.TokenFormat.JsonWebToken;
        }

        public void Configure(string name, OpenIddictServerAspNetCoreOptions options)
        {
            // Note: the OpenID module handles the authorization, logout, token and userinfo requests
            // in its dedicated ASP.NET Core MVC controller, which requires enabling the pass-through mode.
            options.EnableAuthorizationEndpointPassthrough = true;
            options.EnableLogoutEndpointPassthrough = true;
            options.EnableTokenEndpointPassthrough = true;
            options.EnableUserinfoEndpointPassthrough = true;

            // Note: caching is enabled for both authorization and logout requests to allow sending
            // large POST authorization and logout requests, but can be programmatically disabled, as the
            // authorization and logout views support flowing the entire payload and not just the request_id.
            options.EnableAuthorizationRequestCaching = true;
            options.EnableLogoutRequestCaching = true;

            // Note: error pass-through is enabled to allow the actions of the MVC authorization controller
            // to handle the errors returned by the interactive endpoints without relying on the generic
            // status code pages middleware to rewrite the response later in the request processing.
            options.EnableErrorPassthrough = true;

            // Note: in Orchard, transport security is usually configured via the dedicated HTTPS module.
            // To make configuration easier and avoid having to configure it in two different features,
            // the transport security requirement enforced by OpenIddict by default is always turned off.
            options.DisableTransportSecurityRequirement = true;
        }

        public void Configure(OpenIddictServerAspNetCoreOptions options)
            => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
