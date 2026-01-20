using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Configuration;

public sealed class OpenIdClientConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly IOpenIdClientService _clientService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public OpenIdClientConfiguration(
        IOpenIdClientService clientService,
        IDataProtectionProvider dataProtectionProvider,
        ShellSettings shellSettings,
        ILogger<OpenIdClientConfiguration> logger)
    {
        _clientService = clientService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        var settings = GetClientSettingsAsync().GetAwaiter().GetResult();
        if (settings == null)
        {
            return;
        }

        // Register the OpenID Connect client handler in the authentication handlers collection.
        options.AddScheme<OpenIdConnectHandler>(OpenIdConnectDefaults.AuthenticationScheme, settings.DisplayName);
    }

    public void Configure(string name, OpenIdConnectOptions options)
    {
        // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
        if (!string.Equals(name, OpenIdConnectDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        var settings = GetClientSettingsAsync().GetAwaiter().GetResult();
        if (settings == null)
        {
            return;
        }

        options.Authority = settings.Authority.AbsoluteUri;
        options.ClientId = settings.ClientId;
        options.SignedOutRedirectUri = settings.SignedOutRedirectUri ?? options.SignedOutRedirectUri;
        options.SignedOutCallbackPath = settings.SignedOutCallbackPath ?? options.SignedOutCallbackPath;
        options.RequireHttpsMetadata = string.Equals(settings.Authority.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ResponseMode = settings.ResponseMode;
        options.ResponseType = settings.ResponseType;
        options.SaveTokens = settings.StoreExternalTokens;

        options.CallbackPath = settings.CallbackPath ?? options.CallbackPath;

        if (settings.Scopes != null)
        {
            foreach (var scope in settings.Scopes)
            {
                options.Scope.Add(scope);
            }
        }

        if (!string.IsNullOrEmpty(settings.ClientSecret))
        {
            var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));

            try
            {
                options.ClientSecret = protector.Unprotect(settings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The client secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }

        if (settings.Parameters != null && settings.Parameters.Length > 0)
        {
            var parameters = settings.Parameters;
            options.Events.OnRedirectToIdentityProvider = (context) =>
            {
                foreach (var parameter in parameters)
                {
                    context.ProtocolMessage.SetParameter(parameter.Name, parameter.Value);
                }

                return Task.CompletedTask;
            };
        }
    }

    public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

    private async Task<OpenIdClientSettings> GetClientSettingsAsync()
    {
        var settings = await _clientService.GetSettingsAsync();

        var result = await _clientService.ValidateSettingsAsync(settings);

        if (result.Any(x => x != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    var errors = result.Where(x => x != ValidationResult.Success)
                        .Select(x => x.ErrorMessage);

                    _logger.LogWarning("The OpenID client settings are invalid: {Errors}", string.Join(System.Environment.NewLine, errors));
                }

                return null;
            }
        }

        return settings;
    }
}
