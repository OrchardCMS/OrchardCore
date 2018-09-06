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
                    options.FileProviders[i] = new ModuleEmbeddedFileProvider(_applicationContext);
                }
            }

            if (_hostingEnvironment.IsDevelopment())
            {
                options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_applicationContext));
            }
            else
            {
                options.FileProviders.Add(new ApplicationCompiledPageFileProvider(_applicationContext));
            }
        }
    }
}
