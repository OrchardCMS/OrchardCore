using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.GitHub.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Drivers;

/// <summary>
/// Lets the client secret kept in the GitHub authentication settings be moved to a secret.
/// </summary>
public sealed class GitHubSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public GitHubSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<GitHubSecretMigrationDisplayDriver> stringLocalizer)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _dataProtectionProvider = dataProtectionProvider;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    // Several drivers edit the same migration, so each one binds its own fields.
    protected override void BuildPrefix(SecretMigration model, string htmlFieldPrefix)
    {
        base.BuildPrefix(model, htmlFieldPrefix);
        Prefix += ".GitHub";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<GitHubAuthenticationSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["GitHub Authentication"];
            model.DisplayName = S["Client secret"];
            model.SecretName = GitHubConstants.SecretNames.ClientSecret;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<GitHubAuthenticationSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate)
        {
            // The settings reference the secret once every selected credential is saved.
            await migration.MoveToSecretAsync(
                _secretManager,
                _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication),
                settings.ClientSecret,
                model.SecretName,
                S["GitHub Authentication: Client secret"],
                async secretName =>
                {
                    settings.ClientSecretSecretName = secretName;
                    settings.ClientSecret = null;

                    site.Put(settings);
                    await _siteService.UpdateSiteSettingsAsync(site);
                });
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(GitHubAuthenticationSettings settings)
        => !string.IsNullOrWhiteSpace(settings.ClientSecret) && string.IsNullOrWhiteSpace(settings.ClientSecretSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageGitHubAuthentication);
}
