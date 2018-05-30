using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.ResourceManagement.Filters
{
    public class MetaGeneratorFilter : IResultFilter
    {
        private readonly IResourceManager _resourceManager;

        public MetaGeneratorFilter(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult))
            {
                return;
            }
            
            _resourceManager.RegisterMeta(new MetaEntry { Content = "Orchard", Name = "generator" });
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}