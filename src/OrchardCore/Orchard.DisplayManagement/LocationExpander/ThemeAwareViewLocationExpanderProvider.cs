using System.Collections.Generic;
using System.IO;
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
        private static readonly string PageSubPath = "/{1}/{0}" + RazorViewEngine.ViewExtension;
        private const string PageViewsFolder = "/PageViews";

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
            var themeManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IThemeManager>();

            if (themeManager != null)
            {
                var currentThemeId = context.Values["Theme"];

                if (currentThemeId == null)
                {
                    return viewLocations;
                }

                var currentThemeAndBaseThemesOrdered = _extensionManager
                    .GetFeatures(new[] { currentThemeId })
                    .Where(x => x.Extension.Manifest.IsTheme())
                    .Reverse();

                var result = new List<string>();

                if (context.ActionContext.ActionDescriptor is PageActionDescriptor)
                {
                    var pageViewLocations = PageViewLocations().ToList();
                    pageViewLocations.AddRange(viewLocations);
                    return pageViewLocations;

                    IEnumerable<string> PageViewLocations()
                    {
                        foreach (var theme in currentThemeAndBaseThemesOrdered)
                        {
                            var themeViewsPath = "/" + theme.Extension.SubPath.Replace('\\', '/').Trim('/');

                            foreach (var location in viewLocations)
                            {
                                if (location.Contains(PageSubPath))
                                {
                                    yield return location.Replace(location.Substring(0,
                                        location.IndexOf("/{1}/")), themeViewsPath + PageViewsFolder);
                                }
                            }
                        }
                    }
                }

                foreach (var theme in currentThemeAndBaseThemesOrdered)
                {
                    var themeViewsPath = '/' + theme.Extension.SubPath.Replace('\\', '/').Trim('/')
                        + "/Views" + context.AreaName != theme.Id ? '/' + context.AreaName : string.Empty;

                    result.Add(themeViewsPath + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                    result.Add(themeViewsPath + "/Shared/{0}" +RazorViewEngine.ViewExtension);
                }

                result.AddRange(viewLocations);
                return result;
            }

            return viewLocations;
        }
    }
}