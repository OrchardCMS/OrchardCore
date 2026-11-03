using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;
using System.Text.Json.Nodes;

namespace OrchardCore.Twitter.Services;

internal sealed class TwitterSettingsSectionProvider : SiteSettingsSectionProvider<TwitterSettings>
{
    private readonly IDataProtector _protector;

    public TwitterSettingsSectionProvider(IDataProtectionProvider dataProtectionProvider, ISiteService siteService, IShellReleaseManager shellReleaseManager, IStringLocalizer<ISiteSettingsSectionProvider> localizer)
        : base(siteService, shellReleaseManager, localizer)
    {
        _protector = dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
    }

    public override SiteSettingsSectionDescriptor Descriptor { get; } = new()
    {
        Name = "twitter",
        DisplayName = "X (Twitter) integration",
        FeatureId = TwitterConstants.Features.Twitter,
        Description = "Tenant X (Twitter) credentials. Omitted fields retain their values. Every credential but consumerKey is write-only, with a clear property that removes it. Referencing a secret of the Secrets module is done from the admin page; a new value replaces such a reference.",
        RequiresHttps = true,
        RequiresReload = true,
    };

    protected override Permission ManagePermission => Permissions.ManageTwitter;

    protected override void Describe(SiteSettingsSectionProperties<TwitterSettings> properties)
    {
        properties.String("consumerKey", "The API key.", settings => settings.ConsumerKey, (settings, value) => settings.ConsumerKey = value, required: true);
        properties.ProtectedSecret("consumerSecret", "The API key secret.", _protector, settings => settings.ConsumerSecret, (settings, value) => settings.ConsumerSecret = value, settings => settings.ConsumerSecretSecretName, (settings, value) => settings.ConsumerSecretSecretName = value);
        properties.Secret("accessToken", "The access token.", settings => !string.IsNullOrEmpty(settings.AccessToken), (settings, value) => settings.AccessToken = value, settings => settings.AccessToken = null);
        properties.ProtectedSecret("accessTokenSecret", "The access token secret.", _protector, settings => settings.AccessTokenSecret, (settings, value) => settings.AccessTokenSecret = value, settings => settings.AccessTokenSecretSecretName, (settings, value) => settings.AccessTokenSecretSecretName = value);
    }

    protected override Task ValidateAsync(JsonObject values, TwitterSettings proposed, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(proposed.AccessToken))
        {
            errors["accessToken"] = [S["The {0} property is required.", "accessToken"]];
        }

        return Task.CompletedTask;
    }
}
