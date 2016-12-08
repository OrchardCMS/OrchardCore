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
            var themeManager = context.ActionContext.HttpContext.RequestServices.GetService<IThemeManager>();
            context.Values["Theme"] = themeManager.GetThemeAsync().Result.Id;
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var extensionManager = context.ActionContext.HttpContext.RequestServices.GetService<IExtensionManager>();
            var themeManager = context.ActionContext.HttpContext.RequestServices.GetService<IThemeManager>();
            var siteService = context.ActionContext.HttpContext.RequestServices.GetService<ISiteService>();
            var site = siteService.GetSiteSettingsAsync().Result;

            var orderedFeatures = extensionManager.GetExtensions().Features;
            var orderedExtensions = orderedFeatures.Where(f => f.Id == f.Extension.Id).Select(f => f.Extension);
            var extension = orderedExtensions.FirstOrDefault(e => e.Id == context.AreaName);

            var themes = new List<IExtensionInfo>();
            var currentTheme = themeManager.GetThemeAsync().Result;
            var adminThemeId = (string)site.Properties["CurrentAdminThemeName"];

            themes.Add(currentTheme);
            themes.AddRange(GetBaseThemes(currentTheme, adminThemeId, orderedExtensions));
            var result = new List<string>();

            foreach (var theme in themes)
            {
                var themeViewsPath = Path.Combine(Path.DirectorySeparatorChar + theme.SubPath,
                    "Views", context.AreaName != theme.Id ? context.AreaName : String.Empty);

                result.Add(Path.Combine(themeViewsPath, "{1}", "{0}.cshtml"));
                result.Add(Path.Combine(themeViewsPath, "Shared", "{0}.cshtml"));
            }

            if (extension != null)
            {
                var extensionViewsPath = Path.Combine(Path.DirectorySeparatorChar
                    + extension.SubPath, "Views");

                result.Add(Path.Combine(extensionViewsPath, "{1}", "{0}.cshtml"));
                result.Add(Path.Combine(extensionViewsPath, "Shared", "{0}.cshtml"));
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