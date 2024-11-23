using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Models;
using OrchardCore.Localization.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Drivers;

/// <summary>
/// Represents a <see cref="SectionDisplayDriver{TModel,TSection}"/> for the localization settings section in the admin site.
/// </summary>
public sealed class LocalizationSettingsDisplayDriver : SiteDisplayDriver<LocalizationSettings>
{
    public const string GroupId = "localization";

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly INotifier _notifier;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly CultureOptions _cultureOptions;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => GroupId;

    public LocalizationSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        INotifier notifier,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<CultureOptions> cultureOptions,
        IHtmlLocalizer<LocalizationSettingsDisplayDriver> htmlLocalizer,
        IStringLocalizer<LocalizationSettingsDisplayDriver> stringLocalizer
    )
    {
        _shellReleaseManager = shellReleaseManager;
        _notifier = notifier;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _cultureOptions = cultureOptions.Value;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    /// <inheritdocs />
    public override async Task<IDisplayResult> EditAsync(ISite site, LocalizationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCultures))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<LocalizationSettingsViewModel>("LocalizationSettings_Edit", model =>
        {
            model.Cultures = ILocalizationService.GetAllCulturesAndAliases()
                .Select(cultureInfo =>
                {
                    return new CultureEntry
                    {
                        Supported = settings.SupportedCultures.Contains(cultureInfo.Name, StringComparer.OrdinalIgnoreCase),
                        CultureInfo = cultureInfo,
                        IsDefault = string.Equals(settings.DefaultCulture, cultureInfo.Name, StringComparison.OrdinalIgnoreCase)
                    };
                }).ToArray();

            if (!model.Cultures.Any(x => x.IsDefault))
            {
                model.Cultures[0].IsDefault = true;
            }
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    /// <inheritdocs />
    public override async Task<IDisplayResult> UpdateAsync(ISite site, LocalizationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageCultures))
        {
            return null;
        }

        var model = new LocalizationSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var supportedCulture = JConvert.DeserializeObject<string[]>(model.SupportedCultures);
        if (supportedCulture.Length == 0)
        {
            context.Updater.ModelState.AddModelError("SupportedCultures", S["A culture is required"]);
        }

        if (context.Updater.ModelState.IsValid)
        {
            // Invariant culture name is empty so a null value is bound.
            settings.DefaultCulture = model.DefaultCulture ?? string.Empty;
            settings.SupportedCultures = supportedCulture;

            if (!settings.SupportedCultures.Contains(settings.DefaultCulture))
            {
                settings.DefaultCulture = settings.SupportedCultures[0];
            }

            // We always release the tenant for the default culture and also supported cultures to take effect.
            _shellReleaseManager.RequestRelease();

            // We create a transient scope with the newly selected culture to create a notification that will use it instead of the previous culture.
            using (CultureScope.Create(settings.DefaultCulture, ignoreSystemSettings: _cultureOptions.IgnoreSystemSettings))
            {
                await _notifier.WarningAsync(H["The site has been restarted for the settings to take effect."]);
            }
        }

        return await EditAsync(site, settings, context);
    }
}
