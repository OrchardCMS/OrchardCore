using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Orchard.Mvc
{
    public class ModularPageApplicationModelProvider : IPageApplicationModelProvider
    {
        // The order is set to execute after the DefaultPageApplicationModelProvider.
        public int Order => -1000 + 10;

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.PageApplicationModel.Filters.Add(new ModularPageViewEnginePathFilter());
        }

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }
    }
}
