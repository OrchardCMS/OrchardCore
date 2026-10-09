using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Azure.Email.Drivers;

/// <summary>
/// Lets the connection string kept in the Azure Communication Services email settings be moved to a secret.
/// </summary>
public sealed class AzureEmailSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public AzureEmailSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AzureEmailSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".AzureEmail";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<AzureEmailSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Azure Communication Services Email"];
            model.DisplayName = S["Connection string"];
            model.SecretName = AzureEmailConstants.SecretNames.ConnectionString;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<AzureEmailSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate && await migration.MoveToSecretAsync(
            _secretManager,
            _dataProtectionProvider.CreateProtector(AzureEmailOptionsConfiguration.ProtectorName),
            settings.ConnectionString,
            model.SecretName,
            S["Azure Communication Services Email: Connection string"]))
        {
            settings.ConnectionStringSecretName = model.SecretName.Trim();
            settings.ConnectionString = null;

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(AzureEmailSettings settings)
        => !string.IsNullOrWhiteSpace(settings.ConnectionString) && string.IsNullOrWhiteSpace(settings.ConnectionStringSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings);
}
