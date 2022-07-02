using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageApplicationModelProvider : IPageApplicationModelProvider
    {
        private readonly ILookup<string, string> _featureIdsByArea;

        public ModularPageApplicationModelProvider(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor)
        {
            // Available features by area in the current shell.            
            _featureIdsByArea = extensionManager.GetFeatures()
                .Where(f => shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                .ToLookup(f => f.Extension.Id, f => f.Id);
        }

        public int Order => -1000 + 10;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
        }

        // Called the 1st time a page is requested or if any page has been updated.
        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
            // Check if the page belongs to an enabled feature.
            var found = false;

            var area = context.PageApplicationModel.AreaName;
            if (_featureIdsByArea.Contains(area))
            {
                found = true;

                var pageModelType = context.PageApplicationModel.ModelType.AsType();
                var attribute = pageModelType.GetCustomAttributes<FeatureAttribute>(false).FirstOrDefault();
                if (attribute != null)
                {
                    found = _featureIdsByArea[area].Contains(attribute.FeatureName);
                }
            }

            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter(found));
        }
    }
}
