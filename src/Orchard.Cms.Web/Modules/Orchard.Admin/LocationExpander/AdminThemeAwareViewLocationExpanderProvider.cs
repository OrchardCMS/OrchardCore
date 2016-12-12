using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.LocationExpander;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Admin.LocationExpander
{
    public class AdminThemeAwareViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public double Priority => 15D;

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var extensionManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IExtensionManager>();

            var adminService = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IAdminThemeService>();

            var currentAdminThemeId = adminService.GetAdminThemeNameAsync().GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(currentAdminThemeId))
            {
                return Enumerable.Empty<string>();
            }

            var currentThemeAndBaseThemesOrdered = extensionManager
                .GetFeatures(new[] { currentAdminThemeId })
                .Where(x => x.Extension.Manifest.IsTheme());

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

            return Enumerable.Empty<string>();
        }
    }
}