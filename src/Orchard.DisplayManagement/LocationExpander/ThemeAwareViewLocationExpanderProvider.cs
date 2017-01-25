using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Modules.LocationExpander;

namespace Orchard.DisplayManagement.LocationExpander
{
    public class ThemeAwareViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public double Priority => 15D;

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
                var currentTheme = themeManager.GetThemeAsync().GetAwaiter().GetResult();

                if (currentTheme != null)
                {
                    context.Values["Theme"] = currentTheme.Id;
                }
            }
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
                var extensionManager = context
                    .ActionContext
                    .HttpContext
                    .RequestServices
                    .GetService<IExtensionManager>();

                var currentTheme = themeManager.GetThemeAsync().GetAwaiter().GetResult();

                if (currentTheme == null)
                {
                    return Enumerable.Empty<string>();
                }

                var currentThemeAndBaseThemesOrdered = extensionManager
                    .GetFeatures(new[] { currentTheme.Id })
                    .Where(x => x.Extension.Manifest.IsTheme())
                    .Reverse();

                var result = new List<string>();

                foreach (var theme in currentThemeAndBaseThemesOrdered)
                {
                    var themeViewsPath = Path.Combine(
                        Path.DirectorySeparatorChar + theme.Extension.SubPath,
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