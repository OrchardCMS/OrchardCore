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

        public ModularRazorViewEngineOptionsSetup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(RazorViewEngineOptions options)
        {
            options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());

            /*
             * 1- ModuleProjectRazorFileProvider (Development only, to provide files from the file system)
             * 2- ContentRootFileProvider
             * 3- ModuleEmbeddedFileProvider (to provide files from embedded resources)
             */

            options.FileProviders.Add(new ModuleEmbeddedFileProvider(_hostingEnvironment));

            if (_hostingEnvironment.IsDevelopment())
            {
                options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_hostingEnvironment));
            }
        }
    }
}
