using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
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

    private readonly ILocalizationService _localizationService;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => GroupId;

    public LocalizationSettingsDisplayDriver(
        ILocalizationService localizationService,
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<LocalizationSettingsDisplayDriver> stringLocalizer
    )
    {
        _localizationService = localizationService;
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    /// <inheritdocs />
    public override async Task<IDisplayResult> EditAsync(ISite site, LocalizationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, LocalizationPermissions.ManageCultures))
        {
            return null;
        }

        return Initialize<LocalizationSettingsViewModel>("LocalizationSettings_Edit", model =>
        {
            model.Cultures = _localizationService.GetAllCulturesAndAliases()
                .Select(cultureInfo =>
                {
                    return new CultureEntry
                    {
                        Supported = settings.SupportedCultures.Contains(cultureInfo.Name, StringComparer.OrdinalIgnoreCase),
                        CultureInfo = cultureInfo,
                        IsDefault = string.Equals(settings.DefaultCulture, cultureInfo.Name, StringComparison.OrdinalIgnoreCase),
                    };
                }).ToArray();

            if (!model.Cultures.Any(x => x.IsDefault))
            {
                model.Cultures[0].IsDefault = true;
            }

            model.FallBackToParentCultures = settings.FallBackToParentCulture;
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    /// <inheritdocs />
    public override async Task<IDisplayResult> UpdateAsync(ISite site, LocalizationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, LocalizationPermissions.ManageCultures))
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
            settings.FallBackToParentCulture = model.FallBackToParentCultures;
            settings.SupportedCultures = supportedCulture;

            if (!settings.SupportedCultures.Contains(settings.DefaultCulture))
            {
                settings.DefaultCulture = settings.SupportedCultures[0];
            }

            // The request localization options are rebuilt from the new settings once they are committed.
            _optionsUpdateNotifier.RequestUpdate<RequestLocalizationOptions>();
        }

        return await EditAsync(site, settings, context);
    }
}
