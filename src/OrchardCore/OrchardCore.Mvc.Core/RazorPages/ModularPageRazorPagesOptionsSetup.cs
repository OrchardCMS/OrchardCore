using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        private readonly IApplicationContext _applicationContext;

        public ModularPageRazorPagesOptionsSetup(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Configure(RazorPagesOptions options)
        {
            // Only serve pages under the "Areas" folder and whose routes have an area name.
            options.Conventions.AddFolderRouteModelConvention("/", model => model.Selectors.Clear());

            // Add a custom folder route to serve the application's module pages from the root.
            options.Conventions.AddAreaFolderRoute(_applicationContext.Application.Name, "/", "");
        }
    }
}