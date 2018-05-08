using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
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

            ModuleEmbeddedFileProvider embeddedFileProvider = null;

            if (_hostingEnvironment.ContentRootFileProvider is CompositeFileProvider composite)
            {
                foreach (var provider in composite.FileProviders)
                {
                    if (provider is ModuleEmbeddedFileProvider embedded)
                    {
                        embeddedFileProvider = embedded;
                        break;
                    }
                }
            }

            for (var i = 0; i < options.FileProviders.Count; i++)
            {
                if (options.FileProviders[i] == _hostingEnvironment.ContentRootFileProvider)
                {
                    options.FileProviders[i] = embeddedFileProvider ?? new ModuleEmbeddedFileProvider(_hostingEnvironment);
                }
            }

            if (_hostingEnvironment.IsDevelopment())
            {
                options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_hostingEnvironment));
            }
        }
    }
}
