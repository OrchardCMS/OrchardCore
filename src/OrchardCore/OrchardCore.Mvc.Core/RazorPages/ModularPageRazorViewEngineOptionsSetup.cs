using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorViewEngineOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ModularPageRazorViewEngineOptionsSetup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(RazorViewEngineOptions options)
        {
            if (_hostingEnvironment.ContentRootFileProvider != null)
            {
                for (var i = 0; i < options.FileProviders.Count; i++)
                {
                    if (options.FileProviders[i] == _hostingEnvironment.ContentRootFileProvider)
                    {
                        options.FileProviders[i] = new ModularPageRazorFileProvider(
                            _hostingEnvironment.ContentRootFileProvider);
                    }
                }
            }
        }
    }
}
