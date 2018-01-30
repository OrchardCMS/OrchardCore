using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Mvc.LocationExpander;

namespace OrchardCore.DisplayManagement.LocationExpander
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
                .Where(x => x.Extension is IThemeExtensionInfo)
                .Reverse();

            if (context.ActionContext.ActionDescriptor is PageActionDescriptor page)
            {
                var pageViewLocations = PageViewLocations().ToList();
                pageViewLocations.AddRange(viewLocations);
                return pageViewLocations;

                IEnumerable<string> PageViewLocations()
                {
                    if (page.RelativePath.Contains("/Pages/") && !page.RelativePath.StartsWith("/Pages/"))
                    {
                        var pageIndex = page.RelativePath.LastIndexOf("/Pages/");
                        var moduleFolder = page.RelativePath.Substring(0, pageIndex);
                        var moduleId = moduleFolder.Substring(moduleFolder.LastIndexOf("/") + 1);

                        foreach (var theme in currentThemeAndBaseThemesOrdered)
                        {
                            if (moduleId != theme.Id)
                            {
                                var themeViewsPath = '/' + theme.Extension.SubPath + "/Views";
                                var themeViewsAreaPath = themeViewsPath + '/' + context.AreaName;
                                yield return themeViewsAreaPath + "/Shared/{0}" + RazorViewEngine.ViewExtension;
                                yield return themeViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension;
                            }
                        }
                    }
                }
            }

            var result = new List<string>();

            if (!String.IsNullOrEmpty(context.AreaName))
            {
                foreach (var theme in currentThemeAndBaseThemesOrdered)
                {
                    if (context.AreaName != theme.Id)
                    {
                        var themeViewsPath = '/' + theme.Extension.SubPath + "/Views";
                        var themeViewsAreaPath = themeViewsPath + '/' + context.AreaName;
                        result.Add(themeViewsAreaPath + "/{1}/{0}" + RazorViewEngine.ViewExtension);
                        result.Add(themeViewsAreaPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
                        result.Add(themeViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
                    }
                }
            }

            result.AddRange(viewLocations);
            return result;
        }
    }
}
