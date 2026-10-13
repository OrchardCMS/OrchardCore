using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using OrchardCore.Environment.Options;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Twitter;

internal static class OptionsUpdateNotifierExtensions
{
    /// <summary>
    /// Requests the update of the options built from the <see cref="TwitterSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestTwitterUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<TwitterSettings>()
            .RequestUpdate<TwitterOptions>(TwitterDefaults.AuthenticationScheme)
            .RequestUpdate<AuthenticationOptions>();

    /// <summary>
    /// Requests the update of the options built from the <see cref="TwitterSigninSettings"/>.
    /// </summary>
    public static IOptionsUpdateNotifier RequestTwitterSigninUpdate(this IOptionsUpdateNotifier notifier)
        => notifier.RequestUpdate<TwitterOptions>(TwitterDefaults.AuthenticationScheme);
}
