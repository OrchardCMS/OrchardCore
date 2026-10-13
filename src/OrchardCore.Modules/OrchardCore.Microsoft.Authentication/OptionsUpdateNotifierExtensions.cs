using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using OrchardCore.Environment.Options;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Settings;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;

namespace OrchardCore.Microsoft.Authentication;

internal static class OptionsUpdateNotifierExtensions
{
    /// <summary>
    /// Requests the update of the options built from the <see cref="MicrosoftAccountSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestMicrosoftAccountUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<MicrosoftAccountSettings>()
            .RequestUpdate<MicrosoftAccountOptions>(MicrosoftAccountDefaults.AuthenticationScheme)
            .RequestUpdate<AuthenticationOptions>();

    /// <summary>
    /// Requests the update of the options built from the <see cref="AzureADSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestAzureADUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<AzureADSettings>()
            .RequestUpdate<MicrosoftIdentityOptions>(MicrosoftIdentityDefaults.AzureAd)
            .RequestUpdate<OpenIdConnectOptions>(AzureADOptionsConfiguration.AzureAdOpenIdConnectScheme)
            .RequestUpdate<AuthenticationOptions>();
}
