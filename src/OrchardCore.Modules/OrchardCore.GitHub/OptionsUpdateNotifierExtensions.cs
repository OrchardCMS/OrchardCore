using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using OrchardCore.Environment.Options;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub;

internal static class OptionsUpdateNotifierExtensions
{
    /// <summary>
    /// Requests the update of the options built from the <see cref="GitHubAuthenticationSettings"/>,
    /// in the order they are built from each other.
    /// </summary>
    public static IOptionsUpdateNotifier RequestGitHubAuthenticationUpdate(this IOptionsUpdateNotifier notifier)
        => notifier
            .RequestUpdate<GitHubAuthenticationSettings>()
            .RequestUpdate<GitHubAuthenticationOptions>(GitHubAuthenticationDefaults.AuthenticationScheme)
            .RequestUpdate<AuthenticationOptions>();
}
