using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";

        protected readonly IStringLocalizer S;

        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public DefaultSiteSettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IStringLocalizer<DefaultSiteSettingsDisplayDriver> stringLocalizer)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            S = stringLocalizer;
        }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            context.Shape.Metadata.Wrappers.Add("Settings_Wrapper__General");

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

            return Task.FromResult<IDisplayResult>(result);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
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
                }

                if (!String.IsNullOrEmpty(site.BaseUrl) && !Uri.TryCreate(site.BaseUrl, UriKind.Absolute, out _))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.BaseUrl), S["The Base url must be a fully qualified URL."]);
                }

                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

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
    }
}
