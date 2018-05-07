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

            if (_hostingEnvironment.ContentRootFileProvider != null)
            {
                for (var i = 0; i < options.FileProviders.Count; i++)
                {
                    if (options.FileProviders[i] == _hostingEnvironment.ContentRootFileProvider)
                    {
                        options.FileProviders[i] = new ModuleEmbeddedFileProvider(_hostingEnvironment);
                    }
                }
            }

            if (_hostingEnvironment.IsDevelopment())
            {
                options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_hostingEnvironment));
            }
        }
    }
}
