using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Mvc.LocationExpander
{
    public class ModularViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private const string CacheKey = "ModuleViewLocationsByViewComponentName)";

        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IMemoryCache _memoryCache;

        public ModularViewLocationExpanderProvider(
            ApplicationPartManager applicationPartManager,
            ShellDescriptor shellDescriptor,
            IExtensionManager extensionManager,
            IMemoryCache memoryCache)
        {
            _applicationPartManager = applicationPartManager;
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
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
                    if (page.RelativePath.Contains("/Pages/") && !page.RelativePath.StartsWith("/Pages/", StringComparison.Ordinal))
                    {
                        yield return page.RelativePath.Substring(0, page.RelativePath.IndexOf("/Pages/", StringComparison.Ordinal))
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

            if (!context.ViewName.StartsWith("Components/", StringComparison.Ordinal))
            {
                result.Add(extensionViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
            }
            else
            {
                if (!_memoryCache.TryGetValue(CacheKey, out IDictionary<string,
                    IEnumerable<string>> moduleViewLocationsByViewComponentName))
                {
                    var modules = _extensionManager.GetFeatures()
                        .Where(f => f.Id == f.Extension.Id &&
                            _shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                        .Select(f => f.Extension);

                    var feature = new ViewComponentFeature();
                    _applicationPartManager.PopulateFeature(feature);

                    var modulesByComponentName = new Dictionary<string, IList<IExtensionInfo>>();

                    foreach (var component in feature.ViewComponents.Select(vc => vc.AsType()))
                    {
                        var module = modules.FirstOrDefault(e => e.Id == component.Assembly.GetName().Name);

                        if (module == null)
                        {
                            continue;
                        }

                        if (!modulesByComponentName.ContainsKey(component.Name))
                        {
                            modulesByComponentName[component.Name] = new List<IExtensionInfo>();
                        }

                        modulesByComponentName[component.Name].Add(module);
                    }

                    var orderedModuleNames = modules.Select(e => e.Id).Reverse().ToList();

                    moduleViewLocationsByViewComponentName = modulesByComponentName
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value
                            .OrderBy(module => orderedModuleNames.IndexOf(module.Id))
                            .Select(module => '/' + module.SubPath + "/Views" + "/Shared/{0}"
                                + RazorViewEngine.ViewExtension));

                    _memoryCache.Set(CacheKey, moduleViewLocationsByViewComponentName);
                }

                var viewComponentName = context.ViewName.Substring(0, context.ViewName.LastIndexOf('/'))
                    .Substring("Components/".Length) + "ViewComponent";

                if (moduleViewLocationsByViewComponentName.ContainsKey(viewComponentName))
                {
                    result.AddRange(moduleViewLocationsByViewComponentName[viewComponentName]);
                }
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}
