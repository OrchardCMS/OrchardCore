using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Extensions;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.Mvc.Razor
{
    public class ModuleViewLocationExpander : IViewLocationExpander
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

            // GetEnabledFeaturesAsync() updated to be ordered by deps and priorities
            var orderedFeatures = shellFeaturesManager.GetEnabledFeaturesAsync().Result;
            var orderedExtensions = orderedFeatures.Where(f => f.Id == f.Extension.Id).Select(f => f.Extension);

            var extensions = new List<IExtensionInfo>();
            var theme = themeManager.GetThemeAsync().Result;

            extensions.Add(theme);
            extensions.AddRange(GetBaseThemes(theme, orderedExtensions));
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

        private IEnumerable<IExtensionInfo> GetBaseThemes(IExtensionInfo theme, IEnumerable<IExtensionInfo> extensions)
        {
            if (theme.Id.Equals("TheAdmin", StringComparison.OrdinalIgnoreCase))
            {
                // Special case: conceptually, the base themes of "TheAdmin" is the list of all enabled themes.
                // This is so that any enabled theme can have controller/action/views in the Admin of the site.
                return extensions.Where(e => e.Manifest.IsTheme() && e.Id != "TheAdmin");
            }
            else
            {
                var list = new List<IExtensionInfo>();
                var theTheme = new ThemeExtensionInfo(theme);
                while (true)
                {
                    if (theme == null)
                        break;

                    if (!theTheme.HasBaseTheme())
                        break;

                    var baseExtension = extensions.FirstOrDefault(e => theTheme.IsBaseThemeFeature(e.Id));
                    if (baseExtension == null)
                    {
                        break;
                    }

                    // Protect against potential infinite loop
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