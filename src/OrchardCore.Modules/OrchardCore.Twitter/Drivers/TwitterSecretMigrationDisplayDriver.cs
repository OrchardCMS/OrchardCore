using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Drivers;

/// <summary>
/// Lets the API secret key and the access token secret kept in the X (Twitter) settings be moved to secrets.
/// </summary>
public sealed class TwitterSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public TwitterSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<TwitterSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".Twitter";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<TwitterSettings>();

        var canMigrateConsumerSecret = CanMigrate(settings.ConsumerSecret, settings.ConsumerSecretSecretName);
        var canMigrateAccessTokenSecret = CanMigrate(settings.AccessTokenSecret, settings.AccessTokenSecretSecretName);

        if ((!canMigrateConsumerSecret && !canMigrateAccessTokenSecret) || !await AuthorizeAsync())
        {
            return null;
        }

        var results = new List<IDisplayResult>();

        if (canMigrateConsumerSecret)
        {
            results.Add(Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
            {
                model.Group = S["X (Twitter)"];
                model.DisplayName = S["API secret key"];
                model.SecretName = TwitterConstants.SecretNames.ConsumerSecret;
            }).Prefix(ConsumerSecretPrefix)
            .Differentiator("TwitterConsumerSecret")
            .Location("Content"));
        }

        if (canMigrateAccessTokenSecret)
        {
            results.Add(Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
            {
                model.Group = S["X (Twitter)"];
                model.DisplayName = S["Access token secret"];
                model.SecretName = TwitterConstants.SecretNames.AccessTokenSecret;
            }).Prefix(AccessTokenSecretPrefix)
            .Differentiator("TwitterAccessTokenSecret")
            .Location("Content"));
        }

        return Combine(results);
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<TwitterSettings>();

        var canMigrateConsumerSecret = CanMigrate(settings.ConsumerSecret, settings.ConsumerSecretSecretName);
        var canMigrateAccessTokenSecret = CanMigrate(settings.AccessTokenSecret, settings.AccessTokenSecretSecretName);

        if ((!canMigrateConsumerSecret && !canMigrateAccessTokenSecret) || !await AuthorizeAsync())
        {
            return null;
        }

        var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
        var updated = false;

        if (canMigrateConsumerSecret)
        {
            var model = new SecretMigrationItemViewModel();
            await context.Updater.TryUpdateModelAsync(model, ConsumerSecretPrefix);

            if (model.Migrate && await migration.MoveToSecretAsync(
                _secretManager,
                protector,
                settings.ConsumerSecret,
                model.SecretName,
                S["X (Twitter): API secret key"]))
            {
                settings.ConsumerSecretSecretName = model.SecretName.Trim();
                settings.ConsumerSecret = null;
                updated = true;
            }
        }

        if (canMigrateAccessTokenSecret)
        {
            var model = new SecretMigrationItemViewModel();
            await context.Updater.TryUpdateModelAsync(model, AccessTokenSecretPrefix);

            if (model.Migrate && await migration.MoveToSecretAsync(
                _secretManager,
                protector,
                settings.AccessTokenSecret,
                model.SecretName,
                S["X (Twitter): Access token secret"]))
            {
                settings.AccessTokenSecretSecretName = model.SecretName.Trim();
                settings.AccessTokenSecret = null;
                updated = true;
            }
        }

        if (updated)
        {
            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return await EditAsync(migration, context);
    }

    private string ConsumerSecretPrefix
        => $"{Prefix}.{nameof(TwitterSettings.ConsumerSecret)}";

    private string AccessTokenSecretPrefix
        => $"{Prefix}.{nameof(TwitterSettings.AccessTokenSecret)}";

    private static bool CanMigrate(string protectedValue, string secretName)
        => !string.IsNullOrWhiteSpace(protectedValue) && string.IsNullOrWhiteSpace(secretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageTwitter);
}
