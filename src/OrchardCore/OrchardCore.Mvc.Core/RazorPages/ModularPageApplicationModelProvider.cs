using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageApplicationModelProvider : IPageApplicationModelProvider
    {
        private IEnumerable<string> _paths;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModularPageApplicationModelProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int Order => -1000 + 10;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            if (_paths != null)
            {
                return;
            }

            lock (this)
            {
                if (_paths == null)
                {
                    var extensionManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IExtensionManager>();
                    var shellDescriptor = _httpContextAccessor.HttpContext.RequestServices.GetService<ShellDescriptor>();

                    // Pages paths of all available modules which are enabled in the current shell.
                    _paths = extensionManager.GetFeatures().Where(f => shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                        .Select(f => '/' + f.Extension.SubPath + "/Pages/").Distinct();
                }
            }
        }

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
            var relativePath = context.ActionDescriptor.RelativePath;
            var found = _paths.Any(p => relativePath.StartsWith(p)) ? true : false;
            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter(found));
        }
    }
}
