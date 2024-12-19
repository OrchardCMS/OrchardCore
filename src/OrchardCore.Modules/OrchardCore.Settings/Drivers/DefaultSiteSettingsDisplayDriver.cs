using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers;

public sealed class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
{
    public const string GroupId = "general";

    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    public DefaultSiteSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<DefaultSiteSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, BuildEditorContext context)
    {
        if (!IsGeneralGroup(context))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        var result = Combine(
            Initialize<SiteSettingsViewModel>("Settings_Edit__Site", model => PopulateProperties(site, model))
                .Location("Content:1#Site;10")
                .OnGroup(GroupId),
            Initialize<SiteSettingsViewModel>("Settings_Edit__Resources", model => PopulateProperties(site, model))
                .Location("Content:1#Resources;20")
                .OnGroup(GroupId),
            Initialize<SiteSettingsViewModel>("Settings_Edit__Cache", model => PopulateProperties(site, model))
                .Location("Content:1#Cache;30")
                .OnGroup(GroupId)
        );

        return result;
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
    {
        if (!IsGeneralGroup(context))
        {
            return null;
        }

        var model = new SiteSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        site.SiteName = model.SiteName;
        site.PageTitleFormat = model.PageTitleFormat;
        site.BaseUrl = model.BaseUrl;
        site.TimeZoneId = model.TimeZone;
        site.PageSize = model.PageSize.Value;
        site.UseCdn = model.UseCdn;
        site.CdnBaseUrl = model.CdnBaseUrl;
        site.ResourceDebugMode = model.ResourceDebugMode;
        site.AppendVersion = model.AppendVersion;
        site.CacheMode = model.CacheMode;

        if (model.PageSize.Value < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.PageSize), S["The page size must be greater than zero."]);
        }

        if (site.MaxPageSize > 0 && model.PageSize.Value > site.MaxPageSize)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.PageSize), S["The page size must be less than or equal to {0}.", site.MaxPageSize]);
        }

        if (!string.IsNullOrEmpty(site.BaseUrl) && !Uri.TryCreate(site.BaseUrl, UriKind.Absolute, out _))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.BaseUrl), S["The Base url must be a fully qualified URL."]);
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, context);
    }

    private static void PopulateProperties(ISite site, SiteSettingsViewModel model)
    {
        model.SiteName = site.SiteName;
        model.PageTitleFormat = site.PageTitleFormat;
        model.BaseUrl = site.BaseUrl;
        model.TimeZone = site.TimeZoneId;
        model.PageSize = site.PageSize;
        model.UseCdn = site.UseCdn;
        model.CdnBaseUrl = site.CdnBaseUrl;
        model.ResourceDebugMode = site.ResourceDebugMode;
        model.AppendVersion = site.AppendVersion;
        model.CacheMode = site.CacheMode;
    }

    private static bool IsGeneralGroup(BuildEditorContext context)
        => context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase);
}
