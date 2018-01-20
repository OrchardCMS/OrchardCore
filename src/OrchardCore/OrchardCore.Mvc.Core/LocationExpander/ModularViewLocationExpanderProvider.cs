using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Mvc.LocationExpander
{
    public class ModularViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IExtensionManager _extensionManager;

        public ModularViewLocationExpanderProvider(ShellDescriptor shellDescriptor, IExtensionManager extensionManager)
        {
            _shellDescriptor = shellDescriptor;
            _extensionManager = extensionManager;
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
                var moduleViewsPaths = _extensionManager.GetFeatures()
                    .Where(f => _shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                    .Select(f => f.Extension).Distinct().Reverse()
                    .Where(e => e.Manifest?.Type?.Equals("module", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Select(e => '/' + e.SubPath + "/Views" + "/Shared/{0}" + RazorViewEngine.ViewExtension);

                result.AddRange(moduleViewsPaths);
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}
