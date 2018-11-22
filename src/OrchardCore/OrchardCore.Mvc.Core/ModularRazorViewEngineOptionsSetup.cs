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

            // To let the application behave as a module, its razor files are requested under the virtual
            // "Areas" folder, but they are still served from the file system by this custom provider.
            options.FileProviders.Insert(0, new ApplicationViewFileProvider(_applicationContext));

            if (_hostingEnvironment.IsDevelopment())
            {
                // While in development, razor files are 1st served from their module project locations.
                options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_applicationContext));
            }
            else
            {
                // In production, the system find application compiled pages under the "Pages" folder.
                // This provider tells to find them under the "Areas/{ApplicationName}/Pages" folder.
                options.FileProviders.Add(new ApplicationCompiledPageFileProvider(_applicationContext));
            }
        }
    }
}
