using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        private readonly IApplicationContext _applicationContext;
        private readonly ShellSettings _shellSettings;

        public ModularPageRazorPagesOptionsSetup(IApplicationContext applicationContext, ShellSettings shellSettings)
        {
            _applicationContext = applicationContext;
            _shellSettings = shellSettings;
        }

        public void Configure(RazorPagesOptions options)
        {
            // Only serve pages under the "Areas" folder and whose routes have an area name.
            options.Conventions.AddFolderRouteModelConvention("/", model => model.Selectors.Clear());

            if (!_shellSettings.IsRunning())
            {
                // Don't serve any page of the application'module which is enabled during a setup.
                options.Conventions.AddAreaFolderRouteModelConvention(_applicationContext.Application.Name, "/",
                    model => model.Selectors.Clear());
            }
            else
            {
                // Add a custom folder route to serve the application's module pages from the root.
                options.Conventions.AddAreaFolderRoute(_applicationContext.Application.Name, "/", "");
            }
        }
    }
}
