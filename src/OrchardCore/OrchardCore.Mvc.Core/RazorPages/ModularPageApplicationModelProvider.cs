using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

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
                    var shellFeaturesManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IShellFeaturesManager>();
                    _paths = shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                        .Select(f => '/' + f.Extension.SubPath + "/Pages/").Distinct();
                }
            }
        }

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
            var viewEnginePath = context.ActionDescriptor.ViewEnginePath;
            var found = _paths.Any(p => viewEnginePath.StartsWith(p)) ? true : false;
            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter(found));
        }
    }
}
