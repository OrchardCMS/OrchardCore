using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.LocationExpander;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Admin.LocationExpander
{
    public class AdminThemeAwareViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public double Priority => 10D;

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var themeManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IThemeManager>();

            if (themeManager != null)
            {
                var shellFeaturesManager = context
                    .ActionContext
                    .HttpContext
                    .RequestServices
                    .GetService<IShellFeaturesManager>();

                var adminService = context
                    .ActionContext
                    .HttpContext
                    .RequestServices
                    .GetService<IAdminThemeService>();

                var currentTheme = themeManager.GetThemeAsync().GetAwaiter().GetResult();
                var currentAdminThemeId = adminService.GetAdminThemeNameAsync().GetAwaiter().GetResult();

                if (currentTheme == null || String.IsNullOrWhiteSpace(currentAdminThemeId)
                    || !currentTheme.Id.Equals(currentAdminThemeId))
                {
                    return Enumerable.Empty<string>();
                }

                var allOtherEnabledThemesOrdered = shellFeaturesManager
                    .GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                    .Select(x => x.Extension)
                    .Where(x => x.Manifest.IsTheme() && x.Id != currentTheme.Id)
                    .ToList();

                var result = new List<string>();

                foreach (var theme in allOtherEnabledThemesOrdered)
                {
                    var themeViewsPath = Path.Combine(
                        Path.DirectorySeparatorChar + theme.SubPath,
                        "Views",
                        context.AreaName != theme.Id ? context.AreaName : string.Empty);

                    result.Add(Path.Combine(themeViewsPath, "{1}", "{0}.cshtml"));
                    result.Add(Path.Combine(themeViewsPath, "Shared", "{0}.cshtml"));
                }

                return result;
            }

            return Enumerable.Empty<string>();
        }
    }
}