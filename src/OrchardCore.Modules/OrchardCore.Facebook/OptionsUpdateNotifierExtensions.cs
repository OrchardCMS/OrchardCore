using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using OrchardCore.Environment.Options;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Facebook;

internal static class OptionsUpdateNotifierExtensions
{
    /// <summary>
    /// Requests the update of the options built from the <see cref="FacebookSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestFacebookUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<FacebookSettings>()
            .RequestUpdate<ResourceManagementOptions>()
            .RequestFacebookLoginUpdate();

    /// <summary>
    /// Requests the update of the options built from the <see cref="FacebookLoginSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestFacebookLoginUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<FacebookOptions>(FacebookDefaults.AuthenticationScheme)
            .RequestUpdate<AuthenticationOptions>();
}
