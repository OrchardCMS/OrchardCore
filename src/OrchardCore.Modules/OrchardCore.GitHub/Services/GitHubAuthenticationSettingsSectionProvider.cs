using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.GitHub.Settings;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Services;

internal sealed class GitHubAuthenticationSettingsSectionProvider : SiteSettingsSectionProvider<GitHubAuthenticationSettings>
{
    private readonly IDataProtector _protector;

    public GitHubAuthenticationSettingsSectionProvider(IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _protector = dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "github-authentication",
        DisplayName = "GitHub authentication",
        FeatureId = GitHubConstants.Features.GitHubAuthentication,
        Description = "Tenant GitHub login. Omitted fields retain their values. clientSecret is write-only; clearClientSecret removes it. Referencing a secret of the Secrets module is done from the admin page; a new clientSecret replaces such a reference.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageGitHubAuthentication;

    protected override void Describe(SiteSettingsSectionProperties<GitHubAuthenticationSettings> properties)
    {
        properties.String("clientId", "The OAuth client identifier.", settings => settings.ClientID, (settings, value) => settings.ClientID = value, required: true);
        properties.ProtectedSecret("clientSecret", "The OAuth client secret.", _protector, settings => settings.ClientSecret, (settings, value) => settings.ClientSecret = value, settings => settings.ClientSecretSecretName, (settings, value) => settings.ClientSecretSecretName = value);
        properties.CallbackPath("callbackPath", "The request path the provider redirects to, or null for the default one.", settings => settings.CallbackPath, (settings, value) => settings.CallbackPath = value);
        properties.Boolean("saveTokens", "Whether to store the access token after a login.", settings => settings.SaveTokens, (settings, value) => settings.SaveTokens = value);
    }
}
