using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Drivers;

/// <summary>
/// Lets the password kept in the SMTP settings be moved to a secret.
/// </summary>
public sealed class SmtpSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public SmtpSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<SmtpSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".Smtp";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<SmtpSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["SMTP"];
            model.DisplayName = S["Password"];
            model.SecretName = SmtpOptionsConfiguration.PasswordSecretName;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<SmtpSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate && await migration.MoveToSecretAsync(
            _secretManager,
            _dataProtectionProvider.CreateProtector(SmtpOptionsConfiguration.ProtectorName),
            settings.Password,
            model.SecretName,
            S["SMTP: Password"]))
        {
            settings.PasswordSecretName = model.SecretName.Trim();
            settings.Password = null;

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(SmtpSettings settings)
        => !string.IsNullOrWhiteSpace(settings.Password) && string.IsNullOrWhiteSpace(settings.PasswordSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings);
}
