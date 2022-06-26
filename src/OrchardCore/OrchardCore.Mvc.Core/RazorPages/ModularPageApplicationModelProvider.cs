using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageApplicationModelProvider : IPageApplicationModelProvider
    {
        private IEnumerable<IFeatureInfo> _features;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public ModularPageApplicationModelProvider(
            ITypeFeatureProvider typeFeatureProvider,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor)
        {
            // Available features in the current shell.            
            _features = extensionManager.GetFeatures().Where(f => shellDescriptor.Features.Any(sf => sf.Id == f.Id));
            _typeFeatureProvider = typeFeatureProvider;
        }

        public int Order => -1000 + 10;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
        }

        // Called the 1st time a page is requested or if any page has been updated.
        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
            // Check if the page belongs to an enabled feature.
            var area = context.PageApplicationModel.AreaName;

            var found = false;
            var featureIdsForArea = _features.Where(f => f.Extension.Id == area).Select(f => f.Id);
            if (featureIdsForArea.Any())
            {
                // All pages with internal model types are available to the module.
                var pageModelType = context.PageApplicationModel.ModelType.AsType();
                if (!IsComponentType(pageModelType))
                {
                    found = true;
                }
                else
                {
                    // Pages with public model types containing the [Feature] attribute
                    // are only available if the feature is enabled.
                    var featureForType = _typeFeatureProvider.GetFeatureForDependency(pageModelType);
                    found = featureIdsForArea.Contains(featureForType.Id);
                }
            }

            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter(found));
        }

        private bool IsComponentType(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsPublic && type != typeof(Object);
        }
    }
}
