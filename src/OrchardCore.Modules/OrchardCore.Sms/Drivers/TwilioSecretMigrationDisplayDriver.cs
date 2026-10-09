using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms.Drivers;

/// <summary>
/// Lets the auth token kept in the Twilio settings be moved to a secret.
/// </summary>
public sealed class TwilioSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public TwilioSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<TwilioSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".Twilio";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<TwilioSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Twilio"];
            model.DisplayName = S["Auth token"];
            model.SecretName = TwilioConstants.SecretNames.AuthToken;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<TwilioSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate && await migration.MoveToSecretAsync(
            _secretManager,
            _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName),
            settings.AuthToken,
            model.SecretName,
            S["Twilio: Auth token"]))
        {
            settings.AuthTokenSecretName = model.SecretName.Trim();
            settings.AuthToken = null;

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(TwilioSettings settings)
        => !string.IsNullOrWhiteSpace(settings.AuthToken) && string.IsNullOrWhiteSpace(settings.AuthTokenSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings);
}
