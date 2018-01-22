using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Mvc.LocationExpander
{
    public class ModularViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private const string CacheKey = nameof(ModularViewLocationExpanderProvider);

        private readonly ShellDescriptor _shellDescriptor;
        private readonly IExtensionManager _extensionManager;
        private readonly IMemoryCache _memoryCache;

        public ModularViewLocationExpanderProvider(
            ShellDescriptor shellDescriptor,
            IExtensionManager extensionManager,
            IMemoryCache memoryCache)
        {
            _shellDescriptor = shellDescriptor;
            _extensionManager = extensionManager;
            _memoryCache = memoryCache;
        }

        public int Priority => 5;

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            if (context.ActionContext.ActionDescriptor is PageActionDescriptor page)
            {
                var pageViewLocations = PageViewLocations().ToList();
                pageViewLocations.AddRange(viewLocations);
                return pageViewLocations;

                IEnumerable<string> PageViewLocations()
                {
                    if (page.RelativePath.Contains("/Pages/") && !page.RelativePath.StartsWith("/Pages/"))
                    {
                        yield return page.RelativePath.Substring(0, page.RelativePath.IndexOf("/Pages/"))
                            + "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;
                    }
                }
            }

            // Get Extension, and then add in the relevant views.
            var extension = _extensionManager.GetExtension(context.AreaName);

            if (!extension.Exists)
            {
                return viewLocations;
            }

            var result = new List<string>();

            var extensionViewsPath = '/' + extension.SubPath + "/Views";
            result.Add(extensionViewsPath + "/{1}/{0}" + RazorViewEngine.ViewExtension);

            if (!context.ViewName.StartsWith("Components/"))
            {
                result.Add(extensionViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
            }
            else
            {
                if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<string>  moduleViewsPaths))
                {
                    moduleViewsPaths = _extensionManager.GetFeatures()
                        .Where(f => f.Id == f.Extension.Id && _shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                        .Where(f => f.Extension.Manifest?.Type?.Equals("module", StringComparison.OrdinalIgnoreCase) ?? false)
                        .Select(f => '/' + f.Extension.SubPath + "/Views" + "/Shared/{0}" + RazorViewEngine.ViewExtension)
                        .Reverse();

                    _memoryCache.Set(CacheKey, moduleViewsPaths);
                }

                result.AddRange(moduleViewsPaths);
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}
