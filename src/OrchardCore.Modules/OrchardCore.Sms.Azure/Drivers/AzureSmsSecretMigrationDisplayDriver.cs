using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Sms.Azure.Services;

namespace OrchardCore.Sms.Azure.Drivers;

/// <summary>
/// Lets the connection string kept in the Azure Communication Services SMS settings be moved to a secret.
/// </summary>
public sealed class AzureSmsSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public AzureSmsSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AzureSmsSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".AzureSms";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<AzureSmsSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Azure Communication Services SMS"];
            model.DisplayName = S["Connection string"];
            model.SecretName = AzureSmsConstants.SecretNames.ConnectionString;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<AzureSmsSettings>();

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
                _dataProtectionProvider.CreateProtector(AzureSmsOptionsConfiguration.ProtectorName),
                settings.ConnectionString,
                model.SecretName,
                S["Azure Communication Services SMS: Connection string"],
                async secretName =>
                {
                    settings.ConnectionStringSecretName = secretName;
                    settings.ConnectionString = null;

                    site.Put(settings);
                    await _siteService.UpdateSiteSettingsAsync(site);
                });
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(AzureSmsSettings settings)
        => !string.IsNullOrWhiteSpace(settings.ConnectionString) && string.IsNullOrWhiteSpace(settings.ConnectionStringSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings);
}
