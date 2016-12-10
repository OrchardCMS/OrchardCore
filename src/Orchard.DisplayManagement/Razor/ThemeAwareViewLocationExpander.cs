using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Extensions;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using Orchard.Settings;

namespace Orchard.DisplayManagement.Razor
{
    public class ThemeAwareViewLocationExpander : IViewLocationExpander
    {
        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var themeManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IThemeManager>();

            if (themeManager != null)
            {
                context.Values["Theme"] = themeManager.GetThemeAsync().GetAwaiter().GetResult().Id;
            }
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var result = new List<string>();

            var themeManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IThemeManager>();

            if (themeManager != null)
            {
                var extensionManager = context
                    .ActionContext
                    .HttpContext
                    .RequestServices
                    .GetService<IExtensionManager>();

                var siteService = context
                    .ActionContext
                    .HttpContext
                    .RequestServices
                    .GetService<ISiteService>();

                var themes = new List<IExtensionInfo>();

                var currentTheme = themeManager.GetThemeAsync().GetAwaiter().GetResult();
                themes.Add(currentTheme);

                var site = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
                var adminThemeId = (string)site.Properties["CurrentAdminThemeName"];
                themes.AddRange(GetBaseThemes(currentTheme, adminThemeId, extensionManager.GetExtensions()));

                foreach (var theme in themes)
                {
                    var themeViewsPath = Path.Combine(
                        Path.DirectorySeparatorChar + theme.SubPath,
                        "Views",
                        context.AreaName != theme.Id ? context.AreaName : String.Empty);

                    result.Add(Path.Combine(themeViewsPath, "{1}", "{0}.cshtml"));
                    result.Add(Path.Combine(themeViewsPath, "Shared", "{0}.cshtml"));
                }
            }

            result.AddRange(viewLocations);

            return result;
        }

        private IEnumerable<IExtensionInfo> GetBaseThemes(IExtensionInfo theme, string adminThemeId, IEnumerable<IExtensionInfo> extensions)
        {
            if (theme?.Id.Equals(adminThemeId, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                return extensions.Where(e => e.Manifest.IsTheme() && e.Id != adminThemeId);
            }
            else
            {
                var list = new List<IExtensionInfo>();

                if (theme == null)
                {
                    return list;
                }

                var themeInfo = new ThemeExtensionInfo(theme);

                while (true)
                {
                    if (!themeInfo.HasBaseTheme())
                        break;

                    var baseTheme = extensions.FirstOrDefault(e => themeInfo.IsBaseThemeFeature(e.Id));

                    if (baseTheme == null || list.Contains(baseTheme))
                    {
                        break;
                    }

                    list.Add(baseTheme);
                    theme = baseTheme;
                }

                return list;
            }
        }
    }
}