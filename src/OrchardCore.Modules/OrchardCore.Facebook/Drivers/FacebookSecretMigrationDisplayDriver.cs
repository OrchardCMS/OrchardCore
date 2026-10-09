using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Facebook.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

/// <summary>
/// Lets the app secret kept in the Meta settings be moved to a secret.
/// </summary>
public sealed class FacebookSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public FacebookSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<FacebookSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".Facebook";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<FacebookSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Meta"];
            model.DisplayName = S["App secret"];
            model.SecretName = FacebookConstants.SecretNames.AppSecret;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<FacebookSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate && await migration.MoveToSecretAsync(
            _secretManager,
            _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core),
            settings.AppSecret,
            model.SecretName,
            S["Meta: App secret"]))
        {
            settings.AppSecretSecretName = model.SecretName.Trim();
            settings.AppSecret = null;

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(FacebookSettings settings)
        => !string.IsNullOrWhiteSpace(settings.AppSecret) && string.IsNullOrWhiteSpace(settings.AppSecretSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageFacebookApp);
}
