using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Extensions;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Settings;

namespace Orchard.DisplayManagement.Razor
{
    public class ThemeAwareViewLocationExpander : IViewLocationExpander
    {
        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var shellFeaturesManager = context.ActionContext.HttpContext.RequestServices.GetService<IShellFeaturesManager>();
            var themeManager = context.ActionContext.HttpContext.RequestServices.GetService<IThemeManager>();
            var siteService = context.ActionContext.HttpContext.RequestServices.GetService<ISiteService>();

            var orderedFeatures = shellFeaturesManager.GetEnabledFeaturesAsync().Result;
            var orderedExtensions = orderedFeatures.Where(f => f.Id == f.Extension.Id).Select(f => f.Extension);

            var extensions = new List<IExtensionInfo>();
            var theme = themeManager.GetThemeAsync().Result;
            var site = siteService.GetSiteSettingsAsync().Result;
            var adminThemeId = (string)site.Properties["CurrentAdminThemeName"];

            extensions.Add(theme);
            extensions.AddRange(GetBaseThemes(theme, adminThemeId, orderedExtensions));
            extensions.AddRange(orderedExtensions.Where(e => !e.Manifest.IsTheme()).Reverse());

            var result = new List<string>();
            foreach (var extension in extensions)
            {
                var viewsPath = Path.Combine(Path.DirectorySeparatorChar + extension.SubPath,
                    "Views", context.AreaName != extension.Id ? context.AreaName : String.Empty);

                result.Add(Path.Combine(viewsPath, "{1}", "{0}.cshtml"));
                result.Add(Path.Combine(viewsPath, "Shared", "{0}.cshtml"));
            }

            result.AddRange(viewLocations);
            return result;
        }

        private IEnumerable<IExtensionInfo> GetBaseThemes(IExtensionInfo theme, string adminThemeId, IEnumerable<IExtensionInfo> extensions)
        {
            if (theme.Id.Equals(adminThemeId, StringComparison.OrdinalIgnoreCase))
            {
                return extensions.Where(e => e.Manifest.IsTheme() && e.Id != adminThemeId);
            }
            else
            {
                var list = new List<IExtensionInfo>();
                var themeInfo = new ThemeExtensionInfo(theme);
                while (true)
                {
                    if (theme == null)
                        break;

                    if (!themeInfo.HasBaseTheme())
                        break;

                    var baseExtension = extensions.FirstOrDefault(e => themeInfo.IsBaseThemeFeature(e.Id));
                    if (baseExtension == null)
                    {
                        break;
                    }

                    if (list.Contains(baseExtension))
                    {
                        break;
                    }

                    list.Add(baseExtension);

                    theme = baseExtension;
                }

                return list;
            }
        }
    }
}