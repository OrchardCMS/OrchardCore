using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageApplicationModelProvider : IPageApplicationModelProvider
    {
        private IEnumerable<string> _paths;

        public ModularPageApplicationModelProvider(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor)
        {
            // Pages paths of all available modules which are enabled in the current shell.
            _paths = extensionManager.GetFeatures().Where(f => shellDescriptor.Features.Any(sf =>
                sf.Id == f.Id)).Select(f => '/' + f.Extension.SubPath + "/Pages/").Distinct();
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
            var found = _paths.Any(p => relativePath.StartsWith(p, StringComparison.Ordinal)) ? true : false;
            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter(found));
        }
    }
}
