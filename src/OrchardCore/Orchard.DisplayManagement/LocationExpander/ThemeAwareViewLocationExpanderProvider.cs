using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions;
using Orchard.Mvc.LocationExpander;

namespace Orchard.DisplayManagement.LocationExpander
{
    public class ThemeAwareViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private readonly IExtensionManager _extensionManager;

        public ThemeAwareViewLocationExpanderProvider(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
        }

        public int Priority => 15;

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
            if (!context.Values.ContainsKey("Theme"))
            {
                return viewLocations;
            }

            var currentThemeId = context.Values["Theme"];

            var currentThemeAndBaseThemesOrdered = _extensionManager
                .GetFeatures(new[] { currentThemeId })
                .Where(x => x.Extension.Manifest.IsTheme())
                .Reverse();

            if (context.ActionContext.ActionDescriptor is PageActionDescriptor)
            {
                if (context.PageName != null)
                {
                    var pageViewLocations = PageViewLocations().ToList();
                    pageViewLocations.AddRange(viewLocations);
                    return pageViewLocations;
                }

                return viewLocations;

                IEnumerable<string> PageViewLocations()
                {
                    foreach (var theme in currentThemeAndBaseThemesOrdered)
                    {
                        if (!context.PageName.StartsWith('/' + theme.Id + '/'))
                        {
                            var themeViewsPath = "/" + theme.Extension.SubPath.Replace('\\', '/').Trim('/');
                            yield return themeViewsPath + "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;
                        }
                    }
                }
            }

            var result = new List<string>();

            foreach (var theme in currentThemeAndBaseThemesOrdered)
            {
                if (context.AreaName != theme.Id)
                {
                    var themeViewsPath = '/' + theme.Extension.SubPath.Replace('\\', '/').Trim('/') +
                        "/Views/" + context.AreaName;

                    result.Add(themeViewsPath + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                    result.Add(themeViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
                }
            }

            result.AddRange(viewLocations);
            return result;
        }
    }
}