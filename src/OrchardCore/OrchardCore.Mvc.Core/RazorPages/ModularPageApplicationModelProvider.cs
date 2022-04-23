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
            _features = extensionManager.GetFeatures().Where(f => shellDescriptor.Features.Any(sf =>
                sf.Id == f.Id)).Select(f => f);

            _typeFeatureProvider = typeFeatureProvider;
        }

        public int Order => -1000 + 10;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
        }

        // Called the 1st time a page is requested or if any page has been updated.
        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
            // Check if the page belongs to an enabled module.
            var relativePath = context.ActionDescriptor.RelativePath;
            var found = false;
            var featureForPath = _features.Where(f =>
               relativePath.StartsWith('/' + f.Extension.SubPath + "/Pages/", StringComparison.Ordinal))
                .OrderBy(f => f.Id == f.Extension.Id ? 1 : 0).ToArray();

            // All pages with internal model types are available to module
            var pageModelType = context.PageApplicationModel.ModelType.AsType();
            if (!IsComponentType(pageModelType))
            {
                found = featureForPath.Any(f => f.Id == f.Extension.Id);
            }
            else
            {
                // Pages with public model types containing [Feature] attribute
                // are available only if feature is enabled 
                foreach (var feature in featureForPath)
                {
                    var blueprint = _typeFeatureProvider.GetFeatureForDependency(pageModelType);
                    if (blueprint != null && feature.Id == blueprint.Id)
                    {
                        found = true;
                        break;
                    }
                }
            }

            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter(found));
        }

        private bool IsComponentType(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsPublic;
        }
    }
}
