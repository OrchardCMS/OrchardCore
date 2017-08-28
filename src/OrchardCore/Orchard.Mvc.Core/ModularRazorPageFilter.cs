using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Builders.Models;

namespace Orchard.Mvc
{
    public class ModularRazorPageFilter : IPageFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModularRazorPageFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            var shellBluePrint = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ShellBlueprint>();
            var moduleIds = shellBluePrint.Dependencies.Values.Select(f => f.FeatureInfo.Extension.Id).Distinct();

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