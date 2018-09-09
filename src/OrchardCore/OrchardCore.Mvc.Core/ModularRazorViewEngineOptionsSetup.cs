using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Mvc.LocationExpander;

namespace OrchardCore.Mvc
{
    public class ModularRazorViewEngineOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IApplicationContext _applicationContext;

        public ModularRazorViewEngineOptionsSetup(IHostingEnvironment hostingEnvironment, IApplicationContext applicationContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _applicationContext = applicationContext;
        }

        public void Configure(RazorViewEngineOptions options)
        {
            options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());

            for (var i = 0; i < options.FileProviders.Count; i++)
            {
                if (options.FileProviders[i] == _hostingEnvironment.ContentRootFileProvider)
                {
                    // The 'ContentRootFileProvider' is replaced because all razor files are embedded.
                    // Unless for the application's module for which the below custom provider is used.
                    options.FileProviders[i] = new ModuleEmbeddedFileProvider(_applicationContext);
                }
            }

            // To let the application behave as a module, its razor files are requested under the virtual
            // ".Modules" folder, but there are still served from the file system by this custom provider.
            options.FileProviders.Insert(0, new ApplicationRazorFileProvider(_applicationContext));

            if (_hostingEnvironment.IsDevelopment())
            {
                // While in development, razor files are 1st served from their module project locations.
                options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_applicationContext));
            }
            else
            {
                // In production, the system find application compiled pages under the application content root.
                // This provider tells that they exist under the virtual ".Modules/ApplicationName/Pages" folder.
                options.FileProviders.Add(new ApplicationCompiledPageFileProvider(_applicationContext));
            }
        }
    }
}
