using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers;

/// <summary>
/// Lets the app secret kept in the Microsoft Account authentication settings be moved to a secret.
/// </summary>
public sealed class MicrosoftAccountSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public MicrosoftAccountSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<MicrosoftAccountSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".MicrosoftAccount";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<MicrosoftAccountSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Microsoft Account Authentication"];
            model.DisplayName = S["App secret"];
            model.SecretName = MicrosoftAuthenticationConstants.SecretNames.AppSecret;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<MicrosoftAccountSettings>();

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
                _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount),
                settings.AppSecret,
                model.SecretName,
                S["Microsoft Account Authentication: App secret"],
                async secretName =>
                {
                    settings.AppSecretSecretName = secretName;
                    settings.AppSecret = null;

                    site.Put(settings);
                    await _siteService.UpdateSiteSettingsAsync(site);
                });
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(MicrosoftAccountSettings settings)
        => !string.IsNullOrWhiteSpace(settings.AppSecret) && string.IsNullOrWhiteSpace(settings.AppSecretSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageMicrosoftAuthentication);
}
