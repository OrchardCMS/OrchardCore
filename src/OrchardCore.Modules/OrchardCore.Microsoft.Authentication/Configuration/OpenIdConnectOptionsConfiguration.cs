using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Validators;
using OrchardCore.Microsoft.Authentication.Settings;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;

namespace OrchardCore.Microsoft.Authentication.Configuration;

internal sealed class OpenIdConnectOptionsConfiguration : IConfigureNamedOptions<OpenIdConnectOptions>
{
    private readonly IOptionsMonitor<MicrosoftIdentityOptions> _azureADOptions;
    private readonly IOptionsMonitor<AzureADSettings> _azureADSettings;

    public OpenIdConnectOptionsConfiguration(
        IOptionsMonitor<MicrosoftIdentityOptions> azureADOptions,
        IOptionsMonitor<AzureADSettings> azureADSettings)
    {
        _azureADOptions = azureADOptions;
        _azureADSettings = azureADSettings;
    }

    public void Configure(string name, OpenIdConnectOptions options)
    {
        if (name != AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme)
        {
            return;
        }

        var azureADOptions = _azureADOptions.Get(MicrosoftIdentityDefaults.AzureAd);

        options.ClientId = azureADOptions.ClientId;
        options.ClientSecret = azureADOptions.ClientSecret;
        options.Authority = new Uri(new Uri(azureADOptions.Instance), azureADOptions.TenantId).ToString();
        options.CallbackPath = azureADOptions.CallbackPath.HasValue ? azureADOptions.CallbackPath : options.CallbackPath;
        options.SignedOutCallbackPath = azureADOptions.SignedOutCallbackPath.HasValue ? azureADOptions.SignedOutCallbackPath : options.SignedOutCallbackPath;
        options.SignInScheme = "Identity.External";
        options.UseTokenLifetime = true;
        options.SaveTokens = _azureADSettings.CurrentValue.SaveTokens;
        options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetAadIssuerValidator(options.Authority, options.Backchannel).Validate;
    }

    public void Configure(OpenIdConnectOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
