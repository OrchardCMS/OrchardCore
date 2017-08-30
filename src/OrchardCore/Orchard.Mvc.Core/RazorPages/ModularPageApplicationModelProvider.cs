using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageApplicationModelProvider : IPageApplicationModelProvider
    {
        public int Order => -1000 + 10;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter());
        }

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }
    }
}
