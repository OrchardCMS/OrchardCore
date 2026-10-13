using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using OpenIddict.Server;
using OpenIddict.Server.DataProtection;
using OpenIddict.Validation;
using OpenIddict.Validation.DataProtection;
using OrchardCore.Environment.Options;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId;

internal static class OptionsUpdateNotifierExtensions
{
    /// <summary>
    /// Requests the update of the options built from the <see cref="OpenIdClientSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestOpenIdClientUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .RequestUpdate<AuthenticationOptions>();

    /// <summary>
    /// Requests the update of the options built from the <see cref="OpenIdServerSettings"/>, including
    /// the validation options of this tenant that can be built from them.
    /// </summary>
    public static IOptionsUpdateNotifier RequestOpenIdServerUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<OpenIddictServerOptions>()
            .RequestUpdate<OpenIddictServerDataProtectionOptions>()
            .RequestOpenIdValidationUpdate();

    /// <summary>
    /// Requests the update of the options built from the <see cref="OpenIdValidationSettings"/>.
    /// </summary>
    public static IOptionsUpdateNotifier RequestOpenIdValidationUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<OpenIddictValidationOptions>()
            .RequestUpdate<OpenIddictValidationDataProtectionOptions>()
            .RequestUpdate<AuthenticationOptions>();
}
