using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orchard.Environment.Shell;

namespace Orchard.Mvc
{
    public class ModularRazorPageFilter : IPageFilter
    {
        private readonly IShellFeaturesManager _shellFeaturesManager;

        public ModularRazorPageFilter(IShellFeaturesManager shellFeaturesManager)
        {
            _shellFeaturesManager = shellFeaturesManager;
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            var moduleIds = _shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                .Select(f => f.Extension.Id).Distinct();

            var moduleId = context.ActionDescriptor.ViewEnginePath.Split(new[] { '/' },
                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (moduleId == null || !moduleIds.Contains(moduleId))
            {
                context.Result = new NotFoundResult();
            }
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }
    }
}