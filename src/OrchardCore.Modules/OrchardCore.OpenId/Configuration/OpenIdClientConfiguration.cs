using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Client.DataProtection;
using OrchardCore.OpenId.Handlers;
using OrchardCore.OpenId.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Configuration;

/// <summary>
/// Configures the OpenIddict client with the registrations of all the <see cref="IOpenIdClientRegistrationProvider"/>:
/// the client registration of the site settings, the registrations managed in the admin and the ones added by
/// other features, e.g. with the OpenIddict web providers.
/// </summary>
public sealed class OpenIdClientConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureOptions<OpenIddictClientOptions>,
    IConfigureOptions<OpenIddictClientDataProtectionOptions>,
    IConfigureNamedOptions<OpenIddictClientAspNetCoreOptions>
{
    private readonly OpenIdClientRegistrationResolver _registrationResolver;
    private readonly IServiceProvider _serviceProvider;

    public OpenIdClientConfiguration(
        OpenIdClientRegistrationResolver registrationResolver,
        IServiceProvider serviceProvider)
    {
        _registrationResolver = registrationResolver;
        _serviceProvider = serviceProvider;
    }

    public void Configure(AuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Note: the OpenIddict client handler is only registered when at least one valid client registration
        // exists, as the OpenIddict client options can't be built without one and would throw at runtime.
        var registrations = _registrationResolver.GetRegistrationsAsync().GetAwaiter().GetResult();
        if (registrations.Count == 0)
        {
            return;
        }

        options.AddScheme<OpenIddictClientAspNetCoreHandler>(
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme, displayName: null);

        // Each client registration gets its own authentication scheme, named after its provider name,
        // that is forwarded to the OpenIddict client handler and listed on the login page.
        foreach (var scheme in _serviceProvider.GetRequiredService<IOptionsMonitor<OpenIddictClientAspNetCoreOptions>>()
            .CurrentValue.ForwardedAuthenticationSchemes)
        {
            options.AddScheme<OpenIddictClientAspNetCoreForwarder>(scheme.Name, scheme.DisplayName);
        }
    }

    public void Configure(OpenIddictClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var registrations = _registrationResolver.GetRegistrationsAsync().GetAwaiter().GetResult();
        if (registrations.Count == 0)
        {
            return;
        }

        // Note: the flows that are allowed to be negotiated by OpenIddict are not configurable globally:
        // the code, hybrid and implicit flows are always enabled, and each registration restricts the
        // response type and response mode it uses.
        options.CodeChallengeMethods.Add(CodeChallengeMethods.Plain);
        options.CodeChallengeMethods.Add(CodeChallengeMethods.Sha256);

        options.GrantTypes.Add(GrantTypes.AuthorizationCode);
        options.GrantTypes.Add(GrantTypes.Implicit);

        options.ResponseModes.Add(ResponseModes.FormPost);
        options.ResponseModes.Add(ResponseModes.Fragment);
        options.ResponseModes.Add(ResponseModes.Query);

        options.ResponseTypes.Add(ResponseTypes.Code);
        options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.IdToken);
        options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.IdToken + ' ' + ResponseTypes.Token);
        options.ResponseTypes.Add(ResponseTypes.Code + ' ' + ResponseTypes.Token);
        options.ResponseTypes.Add(ResponseTypes.IdToken);
        options.ResponseTypes.Add(ResponseTypes.IdToken + ' ' + ResponseTypes.Token);
        options.ResponseTypes.Add(ResponseTypes.Token);

        // Note: the ASP.NET Core Data Protection integration is always used to generate and validate the
        // state tokens produced by the OpenIddict client, so they are protected by the keys of the tenant.
        // To ensure no exception is thrown when no IdentityModel signing/encryption credentials were registered,
        // random symmetric signing and encryption keys are generated and registered here. They are never used
        // to protect a token. The keys used to authenticate the client are attached to each registration.
        options.EncryptionCredentials.Add(new EncryptingCredentials(new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(256 / 8)), SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512));

        options.SigningCredentials.Add(new SigningCredentials(new SymmetricSecurityKey(
            RandomNumberGenerator.GetBytes(256 / 8)), SecurityAlgorithms.HmacSha256));

        options.Handlers.Add(OpenIdChallengeEventHandler.Descriptor);

        options.Registrations.AddRange(registrations);
    }

    public void Configure(OpenIddictClientDataProtectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Note: only the ASP.NET Core Data Protection format is used for the tokens produced by the OpenIddict client.
        options.PreferDefaultStateTokenFormat = false;
    }

    public void Configure(string name, OpenIddictClientAspNetCoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Note: the OpenID module handles the redirection requests in its dedicated
        // ASP.NET Core MVC controller, which requires enabling the pass-through mode.
        options.EnableRedirectionEndpointPassthrough = true;
        options.EnablePostLogoutRedirectionEndpointPassthrough = true;

        // Note: error pass-through is enabled to allow the actions of the MVC callback controller
        // to handle the errors returned by the interactive endpoints without relying on the generic
        // status code pages middleware to rewrite the response later in the request processing.
        options.EnableErrorPassthrough = true;

        // Note: in Orchard, transport security is usually configured via the dedicated HTTPS module.
        // To make configuration easier and avoid having to configure it in two different features,
        // the transport security requirement enforced by OpenIddict by default is always turned off.
        options.DisableTransportSecurityRequirement = true;
    }

    public void Configure(OpenIddictClientAspNetCoreOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
}
